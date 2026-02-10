using Task5.Application.Contracts;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.ValueObjects;

namespace Task5.Application.Services;

internal sealed class CatalogService(
    ILocaleRegistry localeRegistry,
    ISongGenerator songGenerator,
    ISeedCombiner seedCombiner,
    ILikesGenerator likesGenerator) : ICatalogService
{
    public CatalogPageResponse GetPage(CatalogPageRequest request)
    {
        if (!localeRegistry.IsSupported(request.Locale))
            throw new ArgumentException($"Locale '{request.Locale}' is not supported.");

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var localeCode = new LocaleCode(request.Locale);
        var avg = LikesAverage.Create(request.LikesAvg);

        var startIndex = (page - 1) * pageSize + 1;
        var items = new List<SongRowDto>(pageSize);

        for (int i = 0; i < pageSize; i++)
        {
            var index = startIndex + 1;

            var song = songGenerator.GenerateOne(request.Seed, page, localeCode, index);

            var likesSeed = seedCombiner.Combine(request.Seed, 0, index, $"likes|{request.Locale}");
            var likes = likesGenerator.GenerateLikes(likesSeed, avg);

            items.Add(new SongRowDto(
                index,
                song.Title,
                song.Artist,
                song.Album,
                song.Genre,
                likes)
                );
        }

        return new CatalogPageResponse(page, pageSize, items);
    }
}
