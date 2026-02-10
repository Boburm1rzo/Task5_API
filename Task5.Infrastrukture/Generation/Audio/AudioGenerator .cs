using System.Buffers.Binary;
using Task5.Application.Interfaces;
using Task5.Domain.ValueObjects;
using Task5.Infrastructure.Generation.Seeds;

namespace Task5.Infrastructure.Generation.Audio;

internal sealed class AudioGenerator : IAudioGenerator
{
    private const int SampleRate = 44100;
    private const short BitsPerSample = 16;
    private const short Channels = 1;

    public byte[] RenderPreviewWav(Seed64 seed, LocaleCode locale)
    {
        var rng = new DeterministicRng(seed.Value);

        // 6..10 seconds preview (deterministic)
        int seconds = rng.NextInt(6, 11);

        // Tempo 90..150 BPM
        int bpm = rng.NextInt(90, 151);

        // Choose a musical key (MIDI note for root around 48..60 => C3..C4)
        int rootMidi = rng.NextInt(48, 61);

        // Minor/major
        bool minor = rng.Next01() < 0.45;

        // Scale degrees (semitone offsets)
        int[] scale = minor
            ? new[] { 0, 2, 3, 5, 7, 8, 10 }     // natural minor
            : new[] { 0, 2, 4, 5, 7, 9, 11 };    // major

        // Simple chord progression (I–V–vi–IV or i–VII–VI–VII-ish)
        // We keep it deterministic but varied.
        int[][] chordDegrees = minor
            ? new[]
            {
                new[] { 0, 2, 4 }, // i (0,3,7 in semitones inside minor scale mapping)
                new[] { 6, 1, 3 }, // VII-ish
                new[] { 5, 0, 2 }, // VI-ish
                new[] { 6, 1, 3 }  // VII-ish
            }
            : new[]
            {
                new[] { 0, 2, 4 }, // I
                new[] { 4, 6, 1 }, // V
                new[] { 5, 0, 2 }, // vi
                new[] { 3, 5, 0 }  // IV
            };

        // Time grid: 16th notes
        double beatSec = 60.0 / bpm;
        double stepSec = beatSec / 4.0; // 16th
        int totalSteps = (int)Math.Floor(seconds / stepSec);

        // Synthesis buffers
        int totalSamples = seconds * SampleRate;
        var pcm = new short[totalSamples];

        // Voice parameters
        double leadGain = 0.32;
        double bassGain = 0.18;

        // Simple envelope
        double attack = 0.010;
        double release = 0.050;

        // For each step decide note/rest using chord tones + passing tones
        int chordLenSteps = 16; // 1 bar (4 beats) in 16th notes
        int lastMidi = rootMidi;

        for (int step = 0; step < totalSteps; step++)
        {
            int bar = step / chordLenSteps;
            int chordIndex = bar % chordDegrees.Length;

            // Rhythm: more notes on strong positions
            bool strong = (step % 4 == 0);        // quarter note grid
            bool medium = (step % 2 == 0);        // 8th grid

            double noteProb =
                strong ? 0.92 :
                medium ? 0.55 :
                         0.22;

            bool play = rng.Next01() < noteProb;

            // Bass on strong beats (more stable)
            bool bassPlay = strong && (rng.Next01() < 0.95);

            int leadMidi;
            int bassMidi;

            if (play)
            {
                // Pick degree from chord or nearby scale degree
                bool chordTone = rng.Next01() < (strong ? 0.90 : 0.65);

                int degree;
                if (chordTone)
                {
                    var cd = chordDegrees[chordIndex];
                    degree = cd[rng.NextInt(0, cd.Length)];
                }
                else
                {
                    degree = rng.NextInt(0, scale.Length);
                }

                // Map degree to semitone offset from root via scale
                int semitone = scale[degree % scale.Length];

                // Octave choice (lead around root+12..root+24)
                int octaveUp = rng.Next01() < 0.60 ? 12 : 24;
                leadMidi = rootMidi + semitone + octaveUp;

                // Gentle stepwise movement: sometimes nudge toward last note
                if (rng.Next01() < 0.35)
                {
                    int diff = leadMidi - lastMidi;
                    if (diff > 5) leadMidi -= 12;
                    else if (diff < -5) leadMidi += 12;
                }

                lastMidi = leadMidi;
            }
            else
            {
                leadMidi = -1;
            }

            if (bassPlay)
            {
                // Bass plays root of chord (degree 0 of chordDegrees => chord root degree)
                int bassDegree = chordDegrees[chordIndex][0];
                int bassSemi = scale[bassDegree % scale.Length];
                bassMidi = rootMidi + bassSemi; // around C3..C4
            }
            else
            {
                bassMidi = -1;
            }

            // Note duration: mostly 1 step, sometimes 2 or 4 steps
            int durSteps = strong
                ? (rng.Next01() < 0.55 ? 4 : 2)
                : (medium && rng.Next01() < 0.25 ? 2 : 1);

            double durSec = durSteps * stepSec;

            // Render into PCM
            double startSec = step * stepSec;

            if (leadMidi >= 0)
                MixSineNote(pcm, leadMidi, startSec, durSec, leadGain, attack, release);

            if (bassMidi >= 0)
                MixSineNote(pcm, bassMidi, startSec, durSec, bassGain, attack, release);
        }

        // Soft clip + normalize-ish
        SoftLimitInPlace(pcm);

        // Build WAV
        return WriteWavPcm16(pcm, SampleRate, Channels, BitsPerSample);
    }

    private static void MixSineNote(short[] pcm, int midi, double startSec, double durSec,
       double gain, double attack, double release)
    {
        double freq = MidiToHz(midi);

        int start = (int)(startSec * SampleRate);
        int len = (int)(durSec * SampleRate);

        int end = Math.Min(pcm.Length, start + len);
        if (start < 0 || start >= pcm.Length || end <= start) return;

        for (int i = start; i < end; i++)
        {
            double t = (i - start) / (double)SampleRate;

            // ADSR-ish envelope (simple)
            double env;
            if (t < attack) env = t / attack;
            else if (t > durSec - release) env = Math.Max(0, (durSec - t) / release);
            else env = 1.0;

            // Add a tiny 2nd harmonic to sound less like a beep
            double s1 = Math.Sin(2 * Math.PI * freq * t);
            double s2 = 0.25 * Math.Sin(2 * Math.PI * freq * 2 * t);

            double sample = (s1 + s2) * env * gain;

            int v = pcm[i] + (int)(sample * short.MaxValue);
            pcm[i] = (short)Math.Clamp(v, short.MinValue, short.MaxValue);
        }
    }

    private static double MidiToHz(int midi) =>
        440.0 * Math.Pow(2.0, (midi - 69) / 12.0);

    private static void SoftLimitInPlace(short[] pcm)
    {
        // Simple soft limiter to reduce harsh clipping
        for (int i = 0; i < pcm.Length; i++)
        {
            double x = pcm[i] / (double)short.MaxValue;
            // tanh soft clip
            double y = Math.Tanh(1.4 * x);
            pcm[i] = (short)(y * short.MaxValue);
        }
    }

    private static byte[] WriteWavPcm16(short[] samples, int sampleRate, short channels, short bitsPerSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        int dataSize = samples.Length * bytesPerSample;

        int byteRate = sampleRate * channels * bytesPerSample;
        short blockAlign = (short)(channels * bytesPerSample);

        using var ms = new MemoryStream(44 + dataSize);
        Span<byte> header = stackalloc byte[44];

        // RIFF
        header[0] = (byte)'R'; header[1] = (byte)'I'; header[2] = (byte)'F'; header[3] = (byte)'F';
        BinaryPrimitives.WriteInt32LittleEndian(header.Slice(4, 4), 36 + dataSize);
        header[8] = (byte)'W'; header[9] = (byte)'A'; header[10] = (byte)'V'; header[11] = (byte)'E';

        // fmt  chunk
        header[12] = (byte)'f'; header[13] = (byte)'m'; header[14] = (byte)'t'; header[15] = (byte)' ';
        BinaryPrimitives.WriteInt32LittleEndian(header.Slice(16, 4), 16);      // PCM
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(20, 2), 1);       // AudioFormat=PCM
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(22, 2), channels);
        BinaryPrimitives.WriteInt32LittleEndian(header.Slice(24, 4), sampleRate);
        BinaryPrimitives.WriteInt32LittleEndian(header.Slice(28, 4), byteRate);
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(32, 2), blockAlign);
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(34, 2), bitsPerSample);

        // data chunk
        header[36] = (byte)'d'; header[37] = (byte)'a'; header[38] = (byte)'t'; header[39] = (byte)'a';
        BinaryPrimitives.WriteInt32LittleEndian(header.Slice(40, 4), dataSize);

        ms.Write(header);

        // PCM data (little-endian)
        Span<byte> buf = stackalloc byte[2];
        foreach (short s in samples)
        {
            BinaryPrimitives.WriteInt16LittleEndian(buf, s);
            ms.Write(buf);
        }

        return ms.ToArray();
    }
}
