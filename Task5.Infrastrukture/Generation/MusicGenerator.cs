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

        var tempo = random.Next(80, 140);
        var key = random.Next(0, 12);
        var scaleType = (ScaleType)random.Next(0, 4);

        var scale = GetScale(scaleType);
        var progression = ChordProgressions[random.Next(ChordProgressions.Length)];

        var notes = new List<Note>();

        var currentBeat = 0.0;

        foreach (var chordDegree in progression)
        {
            var chordNotes = GetChordNotes(key, scale[0], chordDegree);

            for (int i = 0; i < 4; i++)
            {
                var shouldPlayNote = random.NextDouble() > 0.2;
                if (shouldPlayNote)
                {
                    var noteIndex = random.Next(chordNotes.Length);
                    var midiNote = chordNotes[noteIndex] + 60;
                    var duration = random.NextDouble() < 0.5 ? 0.5 : 1.0;
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

        currentBeat = 0.0;
        foreach (var chordDegree in progression)
        {
            var degreeIndex = SafeDegree(chordDegree, scale[0].Length);
            var bassNote = key + scale[0][degreeIndex] + 36;

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

        currentBeat = 0.0;
        foreach (var chordDegree in progression)
        {
            var chordNotes = GetChordNotes(key, scale[0], chordDegree);

            for (int i = 0; i < 2; i++)
            {
                foreach (var note in chordNotes.Take(3))
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
        var d0 = SafeDegree(degree, scale.Length);
        var d2 = SafeDegree(degree + 2, scale.Length);
        var d4 = SafeDegree(degree + 4, scale.Length);

        var root = key + scale[d0];
        var third = key + scale[d2];
        var fifth = key + scale[d4];

        return [root, third, fifth];
    }

    private static int SafeDegree(int degree, int scaleLen)
    {
        if (scaleLen <= 0) return 0;
        var m = degree % scaleLen;
        return m < 0 ? m + scaleLen : m;
    }
}