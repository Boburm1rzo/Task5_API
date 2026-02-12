using Bogus;
using Task5.Application.Contracts;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.Entities;

namespace Task5.Application.Services;


public sealed class SongDetailsService(
    ILocaleDataProvider localeDataProvider,
    ISeedCombiner seedCombiner) : ISongDetailsService
{
    public SongDetailsDto GetDetails(
        string baseUrl,
        string locale,
        ulong seed,
        double likesAvg,
        int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be at least 1");

        const int pageSize = 20;
        var page = (index - 1) / pageSize + 1;
        var effectiveSeed = seedCombiner.Combine(seed, page);

        var localeData = localeDataProvider.GetLocaleData(locale);
        var faker = new Faker(localeData.BogusLocale);
        Randomizer.Seed = new Random((int)effectiveSeed);

        var positionInPage = (index - 1) % pageSize;
        for (int i = 0; i < positionInPage; i++)
        {
            _ = faker.Random.Int();
            _ = faker.Random.String2(10);
        }

        var title = GenerateSongTitle(faker, localeData);
        var artist = faker.Random.Bool(0.6f)
            ? GenerateBandName(faker, localeData)
            : faker.Name.FullName();
        var album = faker.Random.Bool(0.3f)
            ? "Single"
            : GenerateAlbumTitle(faker, localeData);
        var genre = faker.PickRandom(localeData.Genres);
        var likes = CalculateLikes(faker, likesAvg);

        var review = GenerateReview(faker, localeData, title, artist, genre);
        var durationSeconds = faker.Random.Int(120, 360);
        var releaseYear = faker.Random.Int(1960, 2024).ToString();

        return new SongDetailsDto
        (
            index,
            title,
            artist,
            album,
            genre,
            likes,
            $"/api/songs/{index}/cover?locale={locale}&seed={seed}",
            $"/api/songs/{index}/preview?locale={locale}&seed={seed}",
            $"/api/songs/{index}/lyrics?locale={locale}&seed={seed}",
            review,
            durationSeconds,
            releaseYear
        );
    }

    private string GenerateSongTitle(Faker faker, LocaleData localeData)
    {
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
        var patterns = new[]
        {
            () => $"The {faker.PickRandom(localeData.Nouns)}",
            () => $"{faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
            () => faker.Name.LastName(),
            () => $"{faker.Name.LastName()} {faker.PickRandom(localeData.BandSuffixes)}",
            () => $"{faker.PickRandom(localeData.Djs)} {faker.Name.FirstName()}",
        };

        return faker.PickRandom(patterns)();
    }

    private string GenerateAlbumTitle(Faker faker, LocaleData localeData)
    {
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
        var baseValue = (int)Math.Floor(likesAvg);
        var fraction = likesAvg - baseValue;

        return faker.Random.Double() < fraction
            ? baseValue + 1
            : baseValue;
    }

    private string GenerateReview(Faker faker, LocaleData localeData, string title, string artist, string genre)
    {
        var template = faker.PickRandom(localeData.ReviewTemplates);

        return template
            .Replace("{title}", title)
            .Replace("{artist}", artist)
            .Replace("{genre}", genre);
    }
}