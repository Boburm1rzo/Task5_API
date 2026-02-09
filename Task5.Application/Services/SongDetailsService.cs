using Task5.Application.Contracts;
using Task5.Application.Interfaces;

namespace Task5.Application.Services;

internal class SongDetailsService : ISongDetailsService
{
    public SongDetailsDto GetDetails(string baseUrl, string locale, ulong seed, double likesAvg, int index)
    {
        throw new NotImplementedException();
    }
}
