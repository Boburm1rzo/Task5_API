namespace Task5.Domain.Abstractions;

public interface ISeedCombiner
{
    ulong Combine(ulong userSeed, int pageOrIndex);
}
