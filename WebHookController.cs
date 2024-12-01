using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LineDC.Messaging;

namespace PleasanterWebHook;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class WebHookController : ControllerBase
{
    private static readonly LineBotApp _app;

    static WebHookController()
    {
        var dir = Path.GetDirectoryName(typeof(WebHookController).Assembly.Location)
            ?? AppContext.BaseDirectory;

        var netcoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "";
        var config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(dir, "webhooksettings.json"))
            .AddJsonFile(Path.Combine(dir, $"webhooksettings.{netcoreEnv}.json"))
            .Build();
        var settings = config.Get<WebhookSettings>()
            ?? new WebhookSettings();
        var lineClient = LineMessagingClient.Create(new HttpClient(), settings.LineChannelAccessToken);
        _app = new LineBotApp(lineClient, settings.LineChannelSecret);
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
        await _app.RunAsync(xLineSignature, body);
        return Ok();
    }
}
