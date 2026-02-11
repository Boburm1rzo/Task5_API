using Task5.Domain.Abstractions;

namespace Task5.Infrastructure.Generation.Seeds;

internal sealed class SeedCombiner : ISeedCombiner
{
    private const ulong Multiplier = 6364136223846793005UL;
    private const ulong Increment = 1442695040888963407UL;

    public ulong Combine(ulong userSeed, int pageOrIndex)
    {
        // MAD (Multiply-Add-Divide) operation
        // This ensures same seed + same page always produces same result
        // but different pages produce different results

        unchecked
        {
            var combined = userSeed * Multiplier;
            combined += (ulong)pageOrIndex * Increment;
            return combined;
        }
    }
}
