using Task5.Domain.ValueObjects;

namespace Task5.Application.Interfaces;

public interface ICoverRenderer
{
    byte[] RenderPng(Seed64 seed, LocaleCode locale, string title, string artist);
}
