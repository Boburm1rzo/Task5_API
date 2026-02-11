namespace Task5.Application.Contracts;

public sealed record SongDto(
    int Index,
    string Title,
    string Artist,
    string Album,
    string Genre,
    int Likes,
    string CoverUrl,
    string PreviewUrl,
    string DetailsUrl);
