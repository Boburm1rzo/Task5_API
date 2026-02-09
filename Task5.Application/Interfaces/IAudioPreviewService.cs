namespace Task5.Application.Interfaces;

public interface IAudioPreviewService
{
    byte[] RenderPreviewWav(string locale, ulong seed, int index);
}
