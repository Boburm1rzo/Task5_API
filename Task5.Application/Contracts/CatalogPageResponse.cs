namespace Task5.Application.Contracts;

public sealed record CatalogPageResponse(
    int Page,
    int PageSize,
    IReadOnlyList<SongRowDto> Items);
