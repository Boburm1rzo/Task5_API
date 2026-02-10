using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.ValueObjects;

namespace Task5.Application.Services;

internal sealed class CoverService(
    ICoverRenderer coverRenderer,
    ISeedCombiner seedCombiner,
    ILocaleRegistry localeRegistry,
    ISongGenerator songGenerator) : ICoverService
{
    public byte[] RenderCoverPng(string locale, ulong seed, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!localeRegistry.IsSupported(locale))
            throw new ArgumentException($"Unsupported locale: {locale}", nameof(locale));

        var contentSeed = seedCombiner.Combine(seed, 1, index, $"content|{locale}");
        var song = songGenerator.GenerateOne(seed, 1, new LocaleCode(locale), index);

        var coverSeed = seedCombiner.Combine(seed, 0, index, $"cover|{locale}");

        var cover = coverRenderer.RenderPng(coverSeed, new LocaleCode(locale), song.Title, song.Artist);

        return cover;
    }
}
