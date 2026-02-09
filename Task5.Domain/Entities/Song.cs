namespace Task5.Domain.Entities;

public class Song
{
    public required int Index { get; init; }
    public required string Title { get; init; }
    public required string Artist { get; init; }
    public required string Album { get; init; }
    public required string Genre { get; init; }
}
