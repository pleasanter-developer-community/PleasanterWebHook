using LineDC.Messaging.Webhooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PleasanterWebHook.Controller;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class WebHookController : ControllerBase
{
    private readonly WebhookApplication _line;


    public WebHookController(WebhookApplication line)
    {
        _line = line;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok($"{nameof(WebHookController)} is available !");
    }

    [HttpPost("line")]
    public async Task<IActionResult> Line()
    {
        var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var xLineSignature = Request.Headers["x-line-signature"];
        await _line.RunAsync(xLineSignature, body);
        return Ok();
    }
}
