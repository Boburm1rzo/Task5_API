using Microsoft.Extensions.Configuration;
using Task5.Domain.Abstractions;
using Task5.Domain.Entities;

namespace Task5.Application.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public sealed class LocaleDataProvider : ILocaleDataProvider
{
    private readonly IMemoryCache _cache;
    private readonly string _localesDirectory;
    private readonly ILogger<LocaleDataProvider>? _logger;

    public LocaleDataProvider(
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<LocaleDataProvider>? logger = null)
    {
        _cache = cache;
        _logger = logger;

        var configuredDirectory = configuration["LocalesDirectory"] ?? "Locales";

        _localesDirectory = ResolveLocalesDirectory(configuredDirectory);

        _logger?.LogInformation("Locales directory resolved to: {Directory}", _localesDirectory);

        if (!Directory.Exists(_localesDirectory))
        {
            var errorMessage = $"Locales directory not found: {_localesDirectory}";
            _logger?.LogError(errorMessage);
            throw new DirectoryNotFoundException(errorMessage);
        }

        var localeFiles = Directory.GetFiles(_localesDirectory, "*.json");
        _logger?.LogInformation("Found {Count} locale files: {Files}",
            localeFiles.Length,
            string.Join(", ", localeFiles.Select(Path.GetFileName)));
    }

    private string ResolveLocalesDirectory(string configuredPath)
    {
        // If absolute path, use as-is
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        // Try multiple resolution strategies

        // Strategy 1: Relative to application base directory (most common)
        var baseDirectory = AppContext.BaseDirectory;
        var path1 = Path.Combine(baseDirectory, configuredPath);
        if (Directory.Exists(path1))
        {
            _logger?.LogDebug("Found locales using BaseDirectory: {Path}", path1);
            return Path.GetFullPath(path1);
        }

        // Strategy 2: Relative to current directory
        var currentDirectory = Directory.GetCurrentDirectory();
        var path2 = Path.Combine(currentDirectory, configuredPath);
        if (Directory.Exists(path2))
        {
            _logger?.LogDebug("Found locales using CurrentDirectory: {Path}", path2);
            return Path.GetFullPath(path2);
        }

        // Strategy 3: Look in parent directories (for development)
        var searchDirectory = baseDirectory;
        for (int i = 0; i < 5; i++) // Search up to 5 levels
        {
            var testPath = Path.Combine(searchDirectory, configuredPath);
            if (Directory.Exists(testPath))
            {
                _logger?.LogDebug("Found locales in parent directory: {Path}", testPath);
                return Path.GetFullPath(testPath);
            }

            var parent = Directory.GetParent(searchDirectory);
            if (parent == null) break;
            searchDirectory = parent.FullName;
        }

        var projectPatterns = new[]
       {
            Path.Combine(baseDirectory, "..", "..", "..", "Task5.Infrastrukture", configuredPath),
            Path.Combine(baseDirectory, "..", "..", "Task5.Infrastrukture", configuredPath),
            Path.Combine(baseDirectory, "..", "Task5.Infrastrukture", configuredPath),
            Path.Combine(currentDirectory, "Task5.Infrastrukture", configuredPath),
        };

        foreach (var pattern in projectPatterns)
        {
            var normalizedPath = Path.GetFullPath(pattern);
            if (Directory.Exists(normalizedPath))
            {
                _logger?.LogDebug("Found locales using project pattern: {Path}", normalizedPath);
                return normalizedPath;
            }
        }

        _logger?.LogWarning("Could not resolve locales directory. Tried:");
        _logger?.LogWarning("  - BaseDirectory: {Path}", path1);
        _logger?.LogWarning("  - CurrentDirectory: {Path}", path2);

        return Path.GetFullPath(Path.Combine(baseDirectory, configuredPath));
    }

    public LocaleData GetLocaleData(string localeCode)
    {
        if (string.IsNullOrWhiteSpace(localeCode))
            throw new ArgumentException("Locale code cannot be empty", nameof(localeCode));

        // Try to get from cache
        var cacheKey = $"locale_{localeCode}";
        if (_cache.TryGetValue<LocaleData>(cacheKey, out var cachedData))
        {
            _logger?.LogDebug("Loaded locale '{Locale}' from cache", localeCode);
            return cachedData!;
        }

        // Load from file
        var filePath = Path.Combine(_localesDirectory, $"{localeCode}.json");

        _logger?.LogDebug("Looking for locale file: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            var availableLocales = GetSupportedLocales();
            var errorMessage = $"Locale '{localeCode}' is not supported. " +
                             $"Available locales: {string.Join(", ", availableLocales)}. " +
                             $"Expected file: {filePath}";
            _logger?.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(localeCode));
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<LocaleDataJson>(json)
                ?? throw new InvalidOperationException($"Failed to deserialize locale data for '{localeCode}'");

            var localeData = new LocaleData
            (
                data.BogusLocale,
                data.Adjectives,
                data.Nouns,
                data.Verbs,
                data.Genres
            );

            // Cache for 1 hour
            _cache.Set(cacheKey, localeData, TimeSpan.FromHours(1));

            _logger?.LogInformation("Loaded locale '{Locale}' from {FilePath}", localeCode, filePath);

            return localeData;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading locale file: {FilePath}", filePath);
            throw;
        }
    }

    public List<string> GetSupportedLocales()
    {
        if (!Directory.Exists(_localesDirectory))
        {
            _logger?.LogWarning("Locales directory does not exist: {Directory}", _localesDirectory);
            return new List<string>();
        }

        return Directory.GetFiles(_localesDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList()!;
    }
}