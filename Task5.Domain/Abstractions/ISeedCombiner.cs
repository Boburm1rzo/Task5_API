using Task5.Domain.ValueObjects;

namespace Task5.Domain.Abstractions;

public interface ISeedCombiner
{
    Seed64 Combine(ulong userSeed, int page, int index, string purpose);
}
