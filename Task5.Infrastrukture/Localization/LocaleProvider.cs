using System.Text.Json;
using Task5.Application.Interfaces;

namespace Task5.Infrastructure.Localization;

internal sealed class LocaleProvider : ILocaleTextProvider
{
    private readonly Dictionary<string, Dictionary<string, List<string>>> _cache;

    public LocaleProvider()
    {
        _cache = new(StringComparer.OrdinalIgnoreCase);

        LoadLocale("en-US");
        LoadLocale("ru-RU");
    }


    public IReadOnlyList<string> GetList(string locale, string key)
    {
        if (!_cache.TryGetValue(locale, out var dict))
            throw new ArgumentException($"Locale not loaded: {locale}");

        if (!dict.TryGetValue(key, out var list) || list.Count == 0)
            throw new ArgumentException($"Missing locale key '{key}' for locale '{locale}'");

        return list;
    }

    private void LoadLocale(string locale)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", $"{locale}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Locale resource not found: {path}");

        using var stream = File.OpenRead(path);

        var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stream)
                   ?? throw new InvalidOperationException($"Invalid locale JSON: {locale}");

        _cache[locale] = data;
    }
}
