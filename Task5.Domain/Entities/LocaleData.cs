namespace Task5.Domain.Entities;

public sealed record LocaleData(
    string BogusLocale,
    List<string> Adjectives,
    List<string> Nouns,
    List<string> Verbs,
    List<string> Genres);