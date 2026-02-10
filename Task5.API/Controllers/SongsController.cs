using Microsoft.AspNetCore.Mvc;
using Task5.Application.Interfaces;

namespace Task5.API.Controllers;

[ApiController]
[Route("api/songs")]
public sealed class SongsController(
    ISongDetailsService songDetails,
    ICoverService coverService,
    IAudioPreviewService audioPreview) : ControllerBase
{
    [HttpGet("{index:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetDetails(
        int index,
        [FromQuery] string locale = "en-US",
        [FromQuery] ulong seed = 1,
        [FromQuery] double likesAvg = 3.0,
        [FromQuery] int page = 1)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var dto = songDetails.GetDetails(baseUrl, locale, seed, likesAvg, page, index);
            return Ok(dto);
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

    [HttpGet("{index:int}/cover")]
    [Produces("image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetCover(
        int index,
        [FromQuery] string locale = "en-US",
        [FromQuery] ulong seed = 1)
    {
        try
        {
            var png = coverService.RenderCoverPng(locale, seed, index);
            return File(png, "image/png");
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

    [HttpGet("{index:int}/preview")]
    [Produces("audio/wav")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetPreview(
        int index,
        [FromQuery] string locale = "en-US",
        [FromQuery] ulong seed = 1)
    {
        try
        {
            var wav = audioPreview.RenderPreviewWav(locale, seed, index);
            return File(wav, "audio/wav");
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
