namespace Task5.Application.Services;

using Bogus;
using Task5.Application.Contracts;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.Entities;

/// <summary>
/// Implementation of lyrics service
/// </summary>
public sealed class LyricsService(
    ILocaleDataProvider localeDataProvider,
    ISeedCombiner seedCombiner,
    ISongDetailsService songDetailsService) : ILyricsService
{
    public LyricsDto GenerateLyrics(string locale, ulong seed, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be at least 1");

        var effectiveSeed = seedCombiner.Combine(seed, index);
        var localeData = localeDataProvider.GetLocaleData(locale);
        var faker = new Faker(localeData.BogusLocale);
        Randomizer.Seed = new Random((int)effectiveSeed);

        // Get song details for context
        var details = songDetailsService.GetDetails(string.Empty, locale, seed, 0, index);

        var lines = new List<LyricLine>();
        var currentTime = 0.5; // Start after 0.5 seconds

        // Generate verse 1
        lines.AddRange(GenerateVerse(faker, localeData, ref currentTime, "Verse 1"));
        currentTime += 1.0; // Pause

        // Generate chorus
        var chorusLines = GenerateChorus(faker, localeData, ref currentTime);
        lines.AddRange(chorusLines);
        currentTime += 1.0; // Pause

        // Generate verse 2
        lines.AddRange(GenerateVerse(faker, localeData, ref currentTime, "Verse 2"));
        currentTime += 1.0; // Pause

        // Repeat chorus
        lines.AddRange(GenerateChorus(faker, localeData, ref currentTime));

        return new LyricsDto(lines);
    }

    private List<LyricLine> GenerateVerse(Faker faker, LocaleData localeData, ref double currentTime, string label)
    {
        var lines = new List<LyricLine>
        {
            new(currentTime, $"[{label}]" )
        };
        currentTime += 0.5;

        var lineCount = faker.Random.Int(4, 6);
        for (int i = 0; i < lineCount; i++)
        {
            var line = GenerateLyricLine(faker, localeData);
            lines.Add(new LyricLine(currentTime, line));
            currentTime += 2.5; // ~2.5 seconds per line
        }

        return lines;
    }

    private List<LyricLine> GenerateChorus(Faker faker, LocaleData localeData, ref double currentTime)
    {
        var lines = new List<LyricLine>
        {
            new(currentTime, "[Chorus]" )
        };
        currentTime += 0.5;

        var lineCount = faker.Random.Int(3, 4);
        var chorusLine = GenerateLyricLine(faker, localeData); // Repeat same line

        for (int i = 0; i < lineCount; i++)
        {
            lines.Add(new LyricLine(currentTime, chorusLine));
            currentTime += 2.0;
        }

        return lines;
    }

    private string GenerateLyricLine(Faker faker, LocaleData localeData)
    {
        // Generate poetic-sounding lines
        var patterns = new[]
        {
            () => $"{faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)} in the {faker.PickRandom(localeData.Nouns)}",
            () => $"{faker.PickRandom(localeData.Verbs)} through the {faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
            () => $"When the {faker.PickRandom(localeData.Nouns)} {faker.PickRandom(localeData.Verbs)}",
            () => $"{faker.PickRandom(localeData.Nouns)} and {faker.PickRandom(localeData.Nouns)} collide",
            () => $"In a world of {faker.PickRandom(localeData.Adjectives)} {faker.PickRandom(localeData.Nouns)}",
        };

        return faker.PickRandom(patterns)();
    }
}