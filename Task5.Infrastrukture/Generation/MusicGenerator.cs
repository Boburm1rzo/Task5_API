using Task5.Application.Interfaces;
using Task5.Domain.Entities;
using Task5.Domain.ValueObjects;

namespace Task5.Infrastructure.Generation;

public sealed class MusicGenerator : IMusicGenerator
{
    private static readonly int[][] MajorScale = [[0, 2, 4, 5, 7, 9, 11]];
    private static readonly int[][] MinorScale = [[0, 2, 3, 5, 7, 8, 10]];
    private static readonly int[][] PentatonicScale = [[0, 2, 4, 7, 9]];
    private static readonly int[][] BluesScale = [[0, 3, 5, 6, 7, 10]];

    // Common chord progressions
    private static readonly int[][] ChordProgressions =
    [
        [0, 3, 4, 0],
        [0, 5, 3, 4],
        [0, 4, 5, 3],
        [0, 3, 0, 4],
    ];

    public MusicData GenerateMusic(ulong seed)
    {
        var random = new Random((int)seed);

        // Generate musical parameters
        var tempo = random.Next(80, 140); // 80-140 BPM
        var key = random.Next(0, 12);     // C to B
        var scaleType = (ScaleType)random.Next(0, 4);

        var scale = GetScale(scaleType);
        var progression = ChordProgressions[random.Next(ChordProgressions.Length)];

        var notes = new List<Note>();

        // Generate melody over chord progression
        var currentBeat = 0.0;

        foreach (var chordDegree in progression)
        {
            var chordNotes = GetChordNotes(key, scale[0], chordDegree);

            // Generate melody notes for this chord (4 beats)
            for (int i = 0; i < 4; i++)
            {
                var shouldPlayNote = random.NextDouble() > 0.2; // 80% chance
                if (shouldPlayNote)
                {
                    var noteIndex = random.Next(chordNotes.Length);
                    var midiNote = chordNotes[noteIndex] + 60; // Middle octave
                    var duration = random.NextDouble() < 0.5 ? 0.5 : 1.0; // Half or whole beat
                    var velocity = random.Next(60, 100);

                    notes.Add(new Note
                    (
                        midiNote,
                        currentBeat,
                        duration,
                        velocity));
                }

                currentBeat += 1.0;
            }
        }

        // Add bass line
        currentBeat = 0.0;
        foreach (var chordDegree in progression)
        {
            var bassNote = key + scale[0][chordDegree] + 36; // Low octave

            for (int i = 0; i < 4; i++)
            {
                notes.Add(new Note
                (
                    bassNote,
                    currentBeat,
                    1.0,
                    random.Next(70, 90)));

                currentBeat += 1.0;
            }
        }

        // Add harmony notes
        currentBeat = 0.0;
        foreach (var chordDegree in progression)
        {
            var chordNotes = GetChordNotes(key, scale[0], chordDegree);

            for (int i = 0; i < 2; i++) // Play chord twice per 4 beats
            {
                foreach (var note in chordNotes.Take(3)) // Play triad
                {
                    notes.Add(new Note
                    (
                        note + 60,
                        currentBeat,
                        2.0,
                        random.Next(40, 60)
                    ));
                }

                currentBeat += 2.0;
            }
        }

        return new MusicData
        (
            tempo,
            key,
            scaleType,
            notes.OrderBy(n => n.StartBeat).ToList()
        );
    }

    private int[][] GetScale(ScaleType scaleType)
    {
        return scaleType switch
        {
            ScaleType.Major => MajorScale,
            ScaleType.Minor => MinorScale,
            ScaleType.Pentatonic => PentatonicScale,
            ScaleType.Blues => BluesScale,
            _ => MajorScale
        };
    }

    private int[] GetChordNotes(int key, int[] scale, int degree)
    {
        var root = key + scale[degree % scale.Length];
        var third = key + scale[(degree + 2) % scale.Length];
        var fifth = key + scale[(degree + 4) % scale.Length];

        return [root, third, fifth];
    }
}