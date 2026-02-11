using Microsoft.Extensions.DependencyInjection;
using Task5.Application.Interfaces;
using Task5.Application.Services;
using Task5.Domain.Abstractions;

namespace Task5.Application.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISongGenerationService, SongGenerationService>();
        services.AddScoped<ISongDetailsService, SongDetailsService>();
        services.AddScoped<ICoverService, CoverService>();
        services.AddScoped<IAudioPreviewService, AudioPreviewService>();
        services.AddScoped<ILyricsService, LyricsService>();
        services.AddScoped<IExportService, ExportService>();

        // Register helpers
        services.AddSingleton<ILocaleDataProvider, LocaleDataProvider>();

        // Add memory cache for locale data
        services.AddMemoryCache();

        return services;
    }
}
