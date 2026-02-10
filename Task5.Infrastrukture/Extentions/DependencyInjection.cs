using Microsoft.Extensions.DependencyInjection;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;
using Task5.Infrastructure.Generation.Audio;
using Task5.Infrastructure.Generation.Covers;
using Task5.Infrastructure.Generation.Likes;
using Task5.Infrastructure.Generation.Seeds;
using Task5.Infrastructure.Generation.Songs;
using Task5.Infrastructure.Localization;

namespace Task5.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILocaleRegistry, LocaleRegistry>();
        services.AddSingleton<ILocaleTextProvider, LocaleProvider>();

        services.AddSingleton<ISeedCombiner, SeedCombiner>();

        services.AddSingleton<ISongGenerator, SongGenerator>();
        services.AddSingleton<ILikesGenerator, ProbabilisticLikesGenerator>();
        services.AddSingleton<IReviewGenerator, ReviewTextGenerator>();
        services.AddSingleton<ICoverRenderer, CoverRenderer>();
        services.AddSingleton<IAudioGenerator, AudioGenerator>();

        return services;
    }
}
