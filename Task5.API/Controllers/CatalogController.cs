using Microsoft.AspNetCore.Mvc;
using Task5.Application.Contracts;
using Task5.Application.Interfaces;

namespace Task5.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CatalogController(ICatalogService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CatalogPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<CatalogPageResponse> Get(
       [FromQuery] string locale = "en-US",
       [FromQuery] ulong seed = 1,
       [FromQuery] double likesAvg = 3.0,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 20)
    {
        try
        {
            var req = new CatalogPageRequest(locale, seed, likesAvg, page, pageSize);
            return Ok(service.GetPage(req));
        }
        catch (Exception ex) when (
            ex is ArgumentException ||
            ex is ArgumentOutOfRangeException)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
