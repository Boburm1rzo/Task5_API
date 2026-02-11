using NAudio.Wave;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.Entities;

namespace Task5.Application.Services;

public sealed class AudioPreviewService(
    ISeedCombiner seedCombiner,
    IMusicGenerator musicGenerator) : IAudioPreviewService
{
    public byte[] RenderPreviewWav(string locale, ulong seed, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be at least 1");

        // Calculate effective seed for reproducible music
        var effectiveSeed = seedCombiner.Combine(seed, index);

        // Generate MIDI-like music structure
        var musicData = musicGenerator.GenerateMusic(effectiveSeed);

        // Synthesize to WAV
        return SynthesizeToWav(musicData);
    }

    private byte[] SynthesizeToWav(MusicData musicData)
    {
        const int sampleRate = 44100;
        const int durationSeconds = 15; // 15-second preview
        const int channels = 2; // Stereo

        using var memoryStream = new MemoryStream();
        using var writer = new WaveFileWriter(memoryStream,
            new WaveFormat(sampleRate, 16, channels));

        var totalSamples = sampleRate * durationSeconds;
        var buffer = new float[totalSamples * channels];

        // Generate audio samples
        GenerateAudioSamples(buffer, musicData, sampleRate);

        // Convert float to 16-bit PCM
        var bytes = new byte[buffer.Length * 2];
        for (int i = 0; i < buffer.Length; i++)
        {
            var sample = (short)(buffer[i] * short.MaxValue);
            bytes[i * 2] = (byte)(sample & 0xFF);
            bytes[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        writer.Write(bytes, 0, bytes.Length);
        writer.Flush();

        return memoryStream.ToArray();
    }

    private void GenerateAudioSamples(float[] buffer, MusicData musicData, int sampleRate)
    {
        var tempo = musicData.Tempo;
        var beatsPerSecond = tempo / 60.0;
        var samplesPerBeat = (int)(sampleRate / beatsPerSecond);

        foreach (var note in musicData.Notes)
        {
            var frequency = MidiNoteToFrequency(note.MidiNote);
            var noteDurationSamples = (int)(samplesPerBeat * note.DurationBeats);
            var noteStartSample = (int)(samplesPerBeat * note.StartBeat);

            // Ensure we don't exceed buffer length
            if (noteStartSample >= buffer.Length / 2)
                break;

            // Generate note using simple synthesis (sine wave with envelope)
            for (int i = 0; i < noteDurationSamples && noteStartSample + i < buffer.Length / 2; i++)
            {
                var t = i / (double)sampleRate;
                var envelope = CalculateEnvelope(i, noteDurationSamples);
                var sample = (float)(Math.Sin(2 * Math.PI * frequency * t) * envelope * note.Velocity / 127.0);

                // Stereo - write to both channels
                buffer[(noteStartSample + i) * 2] += sample * 0.3f;     // Left
                buffer[(noteStartSample + i) * 2 + 1] += sample * 0.3f; // Right
            }
        }

        // Normalize
        var maxAmplitude = buffer.Max(Math.Abs);
        if (maxAmplitude > 0)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] /= maxAmplitude;
                buffer[i] *= 0.8f; // Prevent clipping
            }
        }
    }

    private double MidiNoteToFrequency(int midiNote)
    {
        // A4 = MIDI 69 = 440 Hz
        return 440.0 * Math.Pow(2.0, (midiNote - 69) / 12.0);
    }

    private double CalculateEnvelope(int sampleIndex, int totalSamples)
    {
        // ADSR envelope (simplified)
        var attackTime = 0.05;
        var decayTime = 0.1;
        var sustainLevel = 0.7;
        var releaseTime = 0.2;

        var position = sampleIndex / (double)totalSamples;

        if (position < attackTime)
            return position / attackTime;

        if (position < attackTime + decayTime)
            return 1.0 - (position - attackTime) / decayTime * (1.0 - sustainLevel);

        if (position < 1.0 - releaseTime)
            return sustainLevel;

        return sustainLevel * (1.0 - (position - (1.0 - releaseTime)) / releaseTime);
    }
}