using Microsoft.Extensions.DependencyInjection;
using Task5.Application.Interfaces;
using Task5.Application.Services;

namespace Task5.Application.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IAudioPreviewService, AudioPreviewService>();
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<ICoverService, CoverService>();
        services.AddSingleton<ISongDetailsService, SongDetailsService>();

        return services;
    }
}
