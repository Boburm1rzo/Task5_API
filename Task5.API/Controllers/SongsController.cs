using Microsoft.AspNetCore.Mvc;
using Task5.Application.Interfaces;

namespace Task5.API.Controllers;

[Controller]
public class SongsController(IAudioPreviewService service) : ControllerBase
{
    [HttpGet("{index:int}/preview")]
    public IActionResult GetPreview(
    int index,
    [FromQuery] string locale = "en-US",
    [FromQuery] ulong seed = 1)
    {
        var wav = service.RenderPreviewWav(locale, seed, index);
        return File(wav, "audio/wav");
    }

}
