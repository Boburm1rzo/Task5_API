using System.Text;
using Task5.Domain.Abstractions;
using Task5.Domain.ValueObjects;

namespace Task5.Infrastructure.Generation.Seeds;

internal sealed class SeedCombiner : ISeedCombiner
{
    private const ulong A = 6364136223846793005UL;
    private const ulong B = 1442695040888963407UL;
    private const ulong C = 22695477UL;
    private const ulong D = 1103515245UL;

    public Seed64 Combine(ulong userSeed, int page, int index, string purpose)
    {
        unchecked
        {
            var p = (ulong)Math.Max(1, page);
            var i = (ulong)Math.Max(1, index);

            var purposeHash = HashPurpose(purpose);

            var mixed = (A * userSeed) + (B * p) + (C * i) + (D * purposeHash);

            return new Seed64(mixed);
        }
    }

    private static ulong HashPurpose(string purpose)
    {
        if (string.IsNullOrWhiteSpace(purpose))
            return 0;

        const ulong fnvOffset = 14695981039346656037UL;
        const ulong fnvPrime = 1099511628211UL;

        ulong hash = fnvOffset;

        foreach (byte b in Encoding.UTF8.GetBytes(purpose))
        {
            hash ^= b;
            hash *= fnvPrime;
        }

        return hash;
    }
}
