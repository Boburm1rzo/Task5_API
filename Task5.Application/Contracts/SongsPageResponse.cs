namespace Task5.Application.Contracts;

public sealed record SongsPageResponse(
    int Page,
    int PageSize,
    List<SongDto> Songs);