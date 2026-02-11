using Task5.Domain.Entities;

namespace Task5.Domain.Abstractions;


/// <summary>
/// Provider for locale-specific data (words, genres, etc.)
/// NO HARDCODED DATA - all must come from external files/database
/// </summary>
public interface ILocaleDataProvider
{
    /// <summary>
    /// Gets locale-specific data for the given locale code
    /// </summary>
    /// <param name="localeCode">Locale code (e.g., "en", "de", "uk")</param>
    /// <returns>Locale data</returns>
    LocaleData GetLocaleData(string localeCode);

    /// <summary>
    /// Gets all supported locale codes
    /// </summary>
    /// <returns>List of supported locale codes</returns>
    List<string> GetSupportedLocales();
}
