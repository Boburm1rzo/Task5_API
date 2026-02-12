namespace Task5.Application.Services;

using NAudio.Lame;
using NAudio.Wave;
using System.IO.Compression;
using Task5.Application.Interfaces;

public sealed class ExportService(
    IAudioPreviewService audioPreview,
    ISongDetailsService songDetails) : IExportService
{
    public async Task<byte[]> GenerateZipAsync(string locale, ulong seed, int[] songIndices)
    {
        if (songIndices == null || songIndices.Length == 0)
            throw new ArgumentException("Song indices cannot be empty", nameof(songIndices));

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var index in songIndices)
            {
                var details = songDetails.GetDetails(string.Empty, locale, seed, 0, index);

                var wavBytes = audioPreview.RenderPreviewWav(locale, seed, index);

                var mp3Bytes = await ConvertWavToMp3Async(wavBytes);

                var filename = SanitizeFilename($"{details.Artist} - {details.Title}.mp3");

                var entry = archive.CreateEntry(filename, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(mp3Bytes, 0, mp3Bytes.Length);
            }
        }

        return memoryStream.ToArray();
    }

    private async Task<byte[]> ConvertWavToMp3Async(byte[] wavBytes)
    {
        using var wavStream = new MemoryStream(wavBytes);
        using var reader = new WaveFileReader(wavStream);
        using var mp3Stream = new MemoryStream();

        using var writer = new LameMP3FileWriter(mp3Stream, reader.WaveFormat, 128);
        await reader.CopyToAsync(writer);
        writer.Flush();

        return mp3Stream.ToArray();
    }

    private string SanitizeFilename(string filename)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", filename.Split(invalid, StringSplitOptions.RemoveEmptyEntries));

        if (sanitized.Length > 200)
            sanitized = sanitized.Substring(0, 200);

        return sanitized;
    }
}