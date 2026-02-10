using Task5.Application.Interfaces;
using Task5.Domain.ValueObjects;
using Task5.Infrastructure.Generation.Seeds;

namespace Task5.Infrastructure.Generation.Songs;

internal sealed class ReviewTextGenerator(ILocaleTextProvider texts) : IReviewGenerator
{
    public string GenerateReview(Seed64 seed, LocaleCode locale)
    {
        var rng = new DeterministicRng(seed.Value);

        var openers = texts.GetList(locale.Value, "review.openers");
        var middles = texts.GetList(locale.Value, "review.middles");
        var closers = texts.GetList(locale.Value, "review.closers");

        string a = Pick(openers, rng);
        string b = Pick(middles, rng);

        bool threeSentences = rng.Next01() < 0.35;
        string c = threeSentences ? Pick(middles, rng) : Pick(closers, rng);
        string d = Pick(closers, rng);

        return threeSentences
            ? $"{a} {b} {c} {d}"
            : $"{a} {b} {d}";
    }

    private static string Pick(IReadOnlyList<string> list, DeterministicRng rng)
    {
        int idx = rng.NextInt(0, list.Count);
        return list[idx];
    }
}
