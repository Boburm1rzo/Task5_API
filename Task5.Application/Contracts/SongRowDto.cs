namespace Task5.Application.Contracts;

public sealed record SongRowDto(
    int Index,
    string Title,
    string Artist,
    string Album,
    string Genre,
    int Likes);
