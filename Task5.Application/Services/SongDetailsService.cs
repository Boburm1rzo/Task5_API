using Task5.Application.Contracts;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.ValueObjects;

namespace Task5.Application.Services;

internal sealed class SongDetailsService(
    ILocaleRegistry localeRegistry,
    ISongGenerator songGenerator,
    ISeedCombiner seedCombiner,
    ILikesGenerator likesGenerator,
    IReviewGenerator reviewGenerator) : ISongDetailsService
{
    public SongDetailsDto GetDetails(string baseUrl, string locale, ulong seed, double likesAvg, int page, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        page = Math.Max(page, 1);

        if (!localeRegistry.IsSupported(locale))
            throw new ArgumentException($"Locale '{locale}' is not supported.", nameof(locale));

        var localeCode = new LocaleCode(locale);
        var avg = LikesAverage.Create(likesAvg);

        var song = songGenerator.GenerateOne(seed, page, localeCode, index);

        var likesSeed = seedCombiner.Combine(seed, page, index, $"likes|{locale}");
        var likes = likesGenerator.GenerateLikes(likesSeed, avg);

        var reivewSeed = seedCombiner.Combine(seed, page, index, $"review|{locale}");
        var review = reviewGenerator.GenerateReview(reivewSeed, localeCode);

        string coverUrl = $"{baseUrl}/api/songs/{index}/cover?locale={Uri.EscapeDataString(locale)}&seed={seed}&page={page}";
        string previewUrl = $"{baseUrl}/api/songs/{index}/preview?locale={Uri.EscapeDataString(locale)}&seed={seed}";

        return new SongDetailsDto(
            Index: song.Index,
            Title: song.Title,
            Artist: song.Artist,
            Album: song.Album,
            Genre: song.Genre,
            Likes: likes,
            ReviewText: review,
            CoverUrl: coverUrl,
            PreviewUrl: previewUrl
        );
    }
}
