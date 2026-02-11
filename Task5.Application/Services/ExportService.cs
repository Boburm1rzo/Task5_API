namespace Task5.Application.Services;

using NAudio.Lame;
using NAudio.Wave;
using System.IO.Compression;
using Task5.Application.Interfaces;


/// <summary>
/// Implementation of export service
/// </summary>
public sealed class ExportService : IExportService
{
    private readonly IAudioPreviewService _audioPreview;
    private readonly ISongDetailsService _songDetails;

    public ExportService(
        IAudioPreviewService audioPreview,
        ISongDetailsService songDetails)
    {
        _audioPreview = audioPreview;
        _songDetails = songDetails;
    }

    public async Task<byte[]> GenerateZipAsync(string locale, ulong seed, int[] songIndices)
    {
        if (songIndices == null || songIndices.Length == 0)
            throw new ArgumentException("Song indices cannot be empty", nameof(songIndices));

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var index in songIndices)
            {
                // Get song details for filename
                var details = _songDetails.GetDetails(string.Empty, locale, seed, 0, index);

                // Generate WAV audio
                var wavBytes = _audioPreview.RenderPreviewWav(locale, seed, index);

                // Convert WAV to MP3
                var mp3Bytes = await ConvertWavToMp3Async(wavBytes);

                // Create safe filename
                var filename = SanitizeFilename($"{details.Artist} - {details.Title}.mp3");

                // Add to ZIP
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

        // Use NAudio.Lame for MP3 encoding
        using var writer = new LameMP3FileWriter(mp3Stream, reader.WaveFormat, 128);
        await reader.CopyToAsync(writer);
        writer.Flush();

        return mp3Stream.ToArray();
    }

    private string SanitizeFilename(string filename)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", filename.Split(invalid, StringSplitOptions.RemoveEmptyEntries));

        // Limit length
        if (sanitized.Length > 200)
            sanitized = sanitized.Substring(0, 200);

        return sanitized;
    }
}