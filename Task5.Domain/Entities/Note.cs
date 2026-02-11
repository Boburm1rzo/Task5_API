namespace Task5.Domain.Entities;

public sealed record Note(
    int MidiNote,
    double StartBeat,
    double DurationBeats,
    int Velocity);