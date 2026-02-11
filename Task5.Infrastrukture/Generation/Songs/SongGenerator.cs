using Bogus;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.Entities;
using Task5.Domain.ValueObjects;

namespace Task5.Infrastructure.Generation.Songs;

internal sealed class SongGenerator(ISeedCombiner seedCombiner) : ISongGenerator
{
    public Song GenerateOne(ulong userSeed, LocaleCode locale, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        var seed = seedCombiner.Combine(userSeed, index);

        var bogusSeed = FoldToInt(seed);

        var bogusLocale = ToBogusLocale(locale.Value);
        var faker = new Faker(bogusLocale)
        {
            Random = new Randomizer(bogusSeed)
        };

        var title = faker.Commerce.ProductName();

        var artist = faker.Random.Bool() ? faker.Name.FullName() : faker.Company.CompanyName();

        var album = faker.Random.Double() < 0.3 ? "Single" : faker.Commerce.ProductName();

        var genre = PickGenre(faker);

        return new Song
        {
            Index = index,
            Title = title,
            Artist = artist,
            Album = album,
            Genre = genre,
        };
    }

    public IReadOnlyList<Song> GeneratePage(ulong userSeed, int page, int pageSize, LocaleCode locale)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        int startIndex = (page - 1) * pageSize + 1;

        var list = new List<Song>(pageSize);
        for (int i = 0; i < pageSize; i++)
        {
            var index = startIndex + i;
            list.Add(GenerateOne(userSeed, locale, index));
        }

        return list;
    }

    private int FoldToInt(ulong x)
    {
        var lo = (uint)(x & 0xFFFFFFFFUL);
        var hi = (uint)(x >> 32);
        return unchecked((int)(lo ^ hi));
    }

    private string ToBogusLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale)) return "en";

        var parts = locale.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].ToLowerInvariant();

        string lang = parts[0].ToLowerInvariant();
        string region = parts[1].ToUpperInvariant();
        return $"{lang}_{region}";
    }

    private string PickGenre(Faker faker)
    {
        var categories = faker.Commerce.Categories(1);
        if (categories is { Length: > 0 } && !string.IsNullOrWhiteSpace(categories[0]))
            return categories[0];

        return faker.Random.Word();
    }
}
