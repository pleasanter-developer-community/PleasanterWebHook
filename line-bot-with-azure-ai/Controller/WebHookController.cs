using LineDC.Messaging.Webhooks.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using PleasanterWebHook.Model;

namespace PleasanterWebHook.Controller;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class WebHookController : ControllerBase
{
    private readonly ILineBotApp _line;
    private readonly Kernel _kernel;

    public WebHookController(ILineBotApp line, Kernel kernel)
    {

        _kernel = kernel;
        _line = line;
        _line.MessageReceived += async (ev, lineClient) =>
        {
            if (ev.Message is TextEventMessage textMessage)
            {
                var text = textMessage.Text;
                var result = await KernelManager.InvokePromptAsync(_kernel, text)
                    ?? "すみません。問題が発生してお答えできません。";
                await lineClient.ReplyMessageAsync(ev.ReplyToken, [result]);
            }
        };
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
