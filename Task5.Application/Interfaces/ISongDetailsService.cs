using Task5.Application.Contracts;

namespace Task5.Application.Interfaces;

public interface ISongDetailsService
{
    SongDetailsDto GetDetails(string baseUrl, string locale, ulong seed, double likesAvg, int page, int index);
}
