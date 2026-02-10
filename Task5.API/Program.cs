using Task5.API.Extensions;
using Task5.Application.Extentions;
using Task5.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task5 API v1");
        c.RoutePrefix = "swagger"; // default
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
