using Task5.Domain.ValueObjects;

namespace Task5.Domain.Entities;

public sealed record MusicData
(
    int Tempo,
    int Key,
    ScaleType Scale,
    List<Note> Notes);
