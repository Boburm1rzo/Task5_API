using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace Task5.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task5 Music Store Generator API",
                Version = "v1"
            });

            c.SupportNonNullableReferenceTypes();

            c.CustomSchemaIds(type => type.FullName);
        });

        return services;
    }
}
