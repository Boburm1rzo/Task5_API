using Microsoft.AspNetCore.Mvc;
using Task5.Application.Interfaces;

namespace Task5.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LocalesController : ControllerBase
{
    private readonly ILocaleRegistry _locales;

    public LocalesController(ILocaleRegistry locales)
    {
        _locales = locales;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<string>> GetSupportedLocales()
        => Ok(_locales.SupportedLocales());
}