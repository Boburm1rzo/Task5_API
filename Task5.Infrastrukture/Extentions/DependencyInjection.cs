using Microsoft.Extensions.DependencyInjection;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Infrastrukture.Generation.Audio;
using Task5.Infrastrukture.Generation.Covers;
using Task5.Infrastrukture.Generation.Likes;
using Task5.Infrastrukture.Generation.Seeds;
using Task5.Infrastrukture.Generation.Songs;
using Task5.Infrastrukture.Localization;

namespace Task5.Infrastrukture.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrasturcture(this IServiceCollection services)
    {
        services.AddSingleton<IAudioGenerator, AudioGenerator>();
        services.AddSingleton<ISongGenerator, SongGenerator>();
        services.AddSingleton<IReviewGenerator, ReviewTextGenerator>();
        services.AddSingleton<ILikesGenerator, ProbabilisticLikesGenerator>();
        services.AddSingleton<ICoverRenderer, CoverRenderer>();
        services.AddSingleton<ISeedCombiner, SeedCombiner>();
        services.AddSingleton<ILocaleRegistry, LocaleRegistry>();
        services.AddSingleton<ILocaleTextProvider, LocaleProvider>();
        services.AddSingleton<DeterministicRng>();

        return services;
    }
}
