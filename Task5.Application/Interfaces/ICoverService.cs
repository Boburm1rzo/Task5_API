namespace Task5.Application.Interfaces;

public interface ICoverService
{
    byte[] RenderCoverPng(string locale, ulong seed, int index);
}
