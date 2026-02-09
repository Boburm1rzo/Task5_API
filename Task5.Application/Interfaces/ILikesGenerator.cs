using Task5.Domain.ValueObjects;

namespace Task5.Application.Interfaces;

public interface ILikesGenerator
{
    int GenerateLikes(Seed64 seedForLikes, LikesAverage averageLikes);
}
