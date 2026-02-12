using Task5.Domain.Entities;

namespace Task5.Domain.Abstractions;

public interface ILocaleDataProvider
{
    LocaleData GetLocaleData(string localeCode);

    List<string> GetSupportedLocales();
}
