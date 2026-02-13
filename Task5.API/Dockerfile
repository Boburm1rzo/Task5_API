FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Task5.API/Task5.API.csproj Task5.API/
COPY Task5.Application/Task5.Application.csproj Task5.Application/
COPY Task5.Domain/Task5.Domain.csproj Task5.Domain/
COPY Task5.Infrastrukture/Task5.Infrastructure.csproj Task5.Infrastrukture/

RUN dotnet restore Task5.API/Task5.API.csproj

COPY . .

RUN dotnet publish Task5.API/Task5.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Task5.API.dll"]
