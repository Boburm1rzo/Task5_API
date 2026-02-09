using Task5.Domain.ValueObjects;

namespace Task5.Application.Interfaces;

public interface IAudioGenerator
{
    byte[] RenderPreviewWav(Seed64 seed, LocaleCode locale);
}
