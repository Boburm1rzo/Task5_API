using Task5.Application.Interfaces;
using Task5.Domain.ValueObjects;
using Task5.Infrastrukture.Generation.Seeds;

namespace Task5.Infrastrukture.Generation.Likes;

internal sealed class ProbabilisticLikesGenerator : ILikesGenerator
{
    public int GenerateLikes(Seed64 seedForLikes, LikesAverage likesAverage)
    {
        var avg = likesAverage.value;

        if (avg <= 0.0) return 0;
        if (avg >= 10.0) return 10;

        var floor = (int)Math.Floor(avg);
        var frac = avg - floor;

        var rng = new DeterministicRng(seedForLikes.Value);

        int likes = rng.Next01() < frac ? floor + 1 : floor;

        if (likes < 0) return 0;
        if (likes > 10) return 10;

        return likes;
    }
}
