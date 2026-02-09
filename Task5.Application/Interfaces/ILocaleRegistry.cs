namespace Task5.Application.Interfaces;

public interface ILocaleRegistry
{
    bool IsSupported(string locale);
    IReadOnlyList<string> SupportedLocales();
}
