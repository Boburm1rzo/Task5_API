using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Task5.Application.Contracts;
using Task5.Application.Interfaces;

namespace Task5.API.Controllers;

[ApiController]
[Route("api/songs")]
public sealed class SongsController(
    ISongGenerationService songGeneration,
    ISongDetailsService songDetails,
    ICoverService coverService,
    IAudioPreviewService audioPreview,
    ILyricsService lyricsService,
    IExportService exportService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetSongs(
        [FromQuery] string locale = "en",
        [FromQuery] ulong seed = 1,
        [FromQuery, Range(0, 10)] double likesAvg = 3.0,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = songGeneration.GeneratePage(
                baseUrl, locale, seed, likesAvg, page, pageSize);
            return Ok(response);
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

    [HttpGet("{index:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetDetails(
        [FromRoute, Range(1, int.MaxValue)] int index,
        [FromQuery] string locale = "en",
        [FromQuery] ulong seed = 1,
        [FromQuery, Range(0, 10)] double likesAvg = 3.0)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var dto = songDetails.GetDetails(baseUrl, locale, seed, likesAvg, index);
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
        [FromRoute, Range(1, int.MaxValue)] int index,
        [FromQuery] string locale = "en",
        [FromQuery] ulong seed = 1,
        [FromQuery] int size = 256)
    {
        try
        {
            var png = coverService.RenderCoverPng(locale, seed, index, size);
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
        [FromRoute, Range(1, int.MaxValue)] int index,
        [FromQuery] string locale = "en",
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

    [HttpGet("{index:int}/lyrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GetLyrics(
        [FromRoute, Range(1, int.MaxValue)] int index,
        [FromQuery] string locale = "en",
        [FromQuery] ulong seed = 1)
    {
        try
        {
            var lyrics = lyricsService.GenerateLyrics(locale, seed, index);
            return Ok(lyrics);
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

    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportToZip(
        [FromBody] ExportRequest request)
    {
        try
        {
            var zipBytes = await exportService.GenerateZipAsync(
                request.Locale,
                request.Seed,
                request.SongIndices);

            return File(zipBytes, "application/zip", "songs.zip");
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