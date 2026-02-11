using Task5.Domain.Entities;

namespace Task5.Application.Interfaces;

public interface IMusicGenerator
{
    MusicData GenerateMusic(ulong seed);
}