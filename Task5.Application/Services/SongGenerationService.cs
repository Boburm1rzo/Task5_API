namespace Task5.Application.Services;

using Bogus;
using Task5.Application.Contracts;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.Entities;

public sealed class SongGenerationService(
    ILocaleDataProvider localeDataProvider,
    ISeedCombiner seedCombiner) : ISongGenerationService
{
    public SongsPageResponse GeneratePage(
        string baseUrl,
        string locale,
        ulong seed,
        double likesAvg,
        int page,
        int pageSize)
    {
        ValidateParameters(locale, likesAvg, page, pageSize);

        var effectiveSeed = seedCombiner.Combine(seed, page);

        // Get locale-specific data
        var localeData = localeDataProvider.GetLocaleData(locale);

        // Create Faker with effective seed and locale
        var faker = new Faker(localeData.BogusLocale);
        Randomizer.Seed = new Random((int)effectiveSeed);

        var songs = new List<SongDto>();

        for (int i = 0; i < pageSize; i++)
        {
            var globalIndex = (page - 1) * pageSize + i + 1;
            var song = GenerateSong(faker, localeData, globalIndex, likesAvg, baseUrl);
            songs.Add(song);
        }

        return new SongsPageResponse
        (
            page,
            pageSize,
            songs
        );
    }

    private SongDto GenerateSong(
        Faker faker,
        LocaleData localeData,
        int index,
        double likesAvg,
        string baseUrl)
    {
        // Generate song title
        var title = GenerateSongTitle(faker, localeData);

        // Generate artist (mix of band names and personal names)
        var artist = faker.Random.Bool(0.6f)
            ? GenerateBandName(faker, localeData)
            : faker.Name.FullName();

        // Generate album or "Single"
        var album = faker.Random.Bool(0.3f)
            ? "Single"
            : GenerateAlbumTitle(faker, localeData);

        // Pick random genre from locale data
        var genre = faker.PickRandom(localeData.Genres);

        // Calculate likes probabilistically
        var likes = CalculateLikes(faker, likesAvg);

        return new SongDto
        (
            index,
            title,
            artist,
            album,
            genre,
            likes,
            $"{baseUrl}/api/songs/{index}/cover",
            $"{baseUrl}/api/songs/{index}/preview",
            $"{baseUrl}/api/songs/{index}");
    }

    private string GenerateSongTitle(Faker faker, LocaleData localeData)
    {
        // Various patterns for song titles
        var patterns = new[]
        {
            () => $"{faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
            () => $"{faker.PickRandom(localeData.Nouns)} {faker.PickRandom(localeData.Verbs)}",
            () => $"{faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
            () => faker.PickRandom(localeData.Nouns),
            () => $"{faker.PickRandom(localeData.Nouns)} of {faker.PickRandom(localeData.Nouns)}",
        };

        return faker.PickRandom(patterns)();
    }

    private string GenerateBandName(Faker faker, LocaleData localeData)
    {
        // Various patterns for band names
        var patterns = new[]
        {
            () => $"The {faker.PickRandom(localeData.Nouns)}",
            () => $"{faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
            () => faker.Name.LastName(),
            () => $"{faker.Name.LastName()} {faker.PickRandom(new[] { "Band", "Group", "Orchestra", "Ensemble" })}",
            () => $"DJ {faker.Name.FirstName()}",
        };

        return faker.PickRandom(patterns)();
    }

    private string GenerateAlbumTitle(Faker faker, LocaleData localeData)
    {
        // Similar to song titles but different patterns
        var patterns = new[]
        {
            () => $"{faker.PickRandom(localeData.Nouns)} {faker.PickRandom(localeData.Nouns)}",
            () => $"{faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
            () => faker.PickRandom(localeData.Nouns),
            () => $"{faker.Random.Number(1900, 2024)}",
        };

        return faker.PickRandom(patterns)();
    }

    private int CalculateLikes(Faker faker, double likesAvg)
    {
        // Probabilistic calculation for fractional likes
        // Example: likesAvg = 3.7
        // base = 3, fraction = 0.7
        // 70% chance of 4 likes, 30% chance of 3 likes

        var baseValue = (int)Math.Floor(likesAvg);
        var fraction = likesAvg - baseValue;

        return faker.Random.Double() < fraction
            ? baseValue + 1
            : baseValue;
    }

    private void ValidateParameters(string locale, double likesAvg, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(locale))
            throw new ArgumentException("Locale cannot be empty", nameof(locale));

        if (likesAvg < 0 || likesAvg > 10)
            throw new ArgumentOutOfRangeException(nameof(likesAvg), "Likes average must be between 0 and 10");

        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100");
    }
}