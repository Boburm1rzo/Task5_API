using Microsoft.Extensions.DependencyInjection;
using Task5.Application.Interfaces;
using Task5.Application.Services;
using Task5.Domain.Abstractions;
using Task5.Infrastructure.Generation;
using Task5.Infrastructure.Generation.Seeds;
using Task5.Infrastructure.Generation.Songs;

namespace Task5.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        services.AddScoped<ISeedCombiner, SeedCombiner>();
        services.AddScoped<ISongGenerator, SongGenerator>();
        services.AddScoped<IAudioPreviewService, AudioPreviewService>();
        services.AddScoped<ISongDetailsService, SongDetailsService>();
        services.AddScoped<IMusicGenerator, MusicGenerator>();
        services.AddScoped<ILyricsService, LyricsService>();
        services.AddScoped<ISongGenerationService, SongGenerationService>();
        services.AddScoped<ICoverService, CoverService>();
        services.AddScoped<IExportService, ExportService>();


        return services;
    }
}
