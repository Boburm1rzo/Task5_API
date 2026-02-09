using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Domain.ValueObjects;

namespace Task5.Application.Services;

internal sealed class AudioPreviewService(
    ILocaleRegistry localeRegistry,
    ISeedCombiner seedCombiner,
    IAudioGenerator audioGenerator) : IAudioPreviewService
{
    public byte[] RenderPreviewWav(string locale, ulong userSeed, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!localeRegistry.IsSupported(locale))
            throw new ArgumentException($"Unsupported locale: {locale}", nameof(locale));

        var audioSeed = seedCombiner.Combine(userSeed, 0, index, $"audio|{locale}");

        return audioGenerator.RenderPreviewWav(audioSeed, new LocaleCode(locale));
    }
}
