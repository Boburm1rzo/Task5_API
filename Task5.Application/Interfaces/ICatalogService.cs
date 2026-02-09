using Task5.Application.Contracts;

namespace Task5.Application.Interfaces;

public interface ICatalogService
{
    CatalogPageResponse GetPage(CatalogPageRequest request);
}
