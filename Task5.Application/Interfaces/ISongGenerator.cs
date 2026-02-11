using Task5.Domain.Entities;
using Task5.Domain.ValueObjects;

namespace Task5.Application.Interfaces;

public interface ISongGenerator
{
    Song GenerateOne(ulong seed, LocaleCode locale, int index);
    IReadOnlyList<Song> GeneratePage(ulong userSeed, int page, int pageIndex, LocaleCode locale);
}
