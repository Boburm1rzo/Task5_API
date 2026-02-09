namespace Task5.Application.Contracts;

public sealed record SongDetailsDto(
    int Index,
    string Title,
    string Artist,
    string Album,
    string Genre,
    int Likes,
    string ReviewText,
    string CoverUrl,
    string PreviewUrl);