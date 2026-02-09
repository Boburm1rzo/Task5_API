using Task5.Application.Interfaces;

namespace Task5.Infrastrukture.Localization;

internal sealed class LocaleRegistry : ILocaleRegistry
{
    private static readonly string[] Supported =
    [
        "en-US",
        "ru-RU"
    ];

    public bool IsSupported(string locale) =>
      Supported.Contains(locale, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> SupportedLocales() => Supported;
}
