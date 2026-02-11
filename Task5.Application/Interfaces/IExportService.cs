namespace Task5.Application.Interfaces;

public interface IExportService
{
    Task<byte[]> GenerateZipAsync(string locale, ulong seed, int[] songIndices);
}
