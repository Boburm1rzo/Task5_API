namespace Task5.Application.Contracts;

public sealed record CatalogPageRequest(
    string Locale,
    ulong Seed,
    double LikesAvg,
    int Page = 1,
    int PageSize = 20);
