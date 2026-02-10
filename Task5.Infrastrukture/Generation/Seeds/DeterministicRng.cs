namespace Task5.Infrastructure.Generation.Seeds;

public sealed class DeterministicRng
{
    private ulong _state;

    public DeterministicRng(ulong seed) => _state = seed;

    public ulong NextU64()
    {
        var z = (_state += 0x9E3779B97F4A7C15UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    public double Next01()
    {
        return (NextU64() >> 11) * (1.0 / (1UL << 53));
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
        var range = (ulong)(maxExclusive - minInclusive);
        return (int)(NextU64() % range) + minInclusive;
    }
}
