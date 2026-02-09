namespace Task5.Application.Interfaces;

public interface ILocaleTextProvider
{
    IReadOnlyList<string> GetList(string locale, string key);
}
