using Task5.Application.Contracts;

namespace Task5.Application.Interfaces;

public interface ILyricsService
{
    LyricsDto GenerateLyrics(string locale, ulong seed, int index);
}
