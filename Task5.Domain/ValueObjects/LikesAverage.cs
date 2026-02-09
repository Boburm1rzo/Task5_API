namespace Task5.Domain.ValueObjects;

public readonly record struct LikesAverage(double value)
{
    public static LikesAverage Create(double v)
    {
        if (v < 0 || v > 10) throw new ArgumentOutOfRangeException(nameof(v), "Likes avg must be 0..10");
        return new LikesAverage(v);
    }
}
