namespace Task5.Application.Contracts;

public record ExportRequest(
    string Locale,
    ulong Seed,
    int[] SongIndices);