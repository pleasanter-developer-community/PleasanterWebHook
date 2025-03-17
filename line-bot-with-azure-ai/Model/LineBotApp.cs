using LineDC.Messaging;
using LineDC.Messaging.Webhooks;
using LineDC.Messaging.Webhooks.Events;

namespace PleasanterWebHook.Model;

public interface ILineBotApp:IWebhookApplication
{
    event Func<MessageEvent, ILineMessagingClient, Task>? MessageReceived;
}

public class LineBotApp : WebhookApplication, ILineBotApp
{
    public LineBotApp(ILineMessagingClient client, string channelSecret)
        : base(client, channelSecret)
    { }

    public event Func<MessageEvent, ILineMessagingClient, Task>? MessageReceived;

    protected async override Task OnJoinAsync(JoinEvent ev)
    {
        await Client.ReplyMessageAsync(ev.ReplyToken, ev.Source.Type switch
        {
            EventSourceType.Group => $$"""
                ようこそ！このグループのIDはこちらです。
                Group ID: {{ev.Source.Id}}
                """,
            EventSourceType.Room => $$"""
                ようこそ！このトークルームのIDはこちらです。
                Room ID: {{ev.Source.Id}}
                """,
            _ => $$"""
                ようこそ！あなたのIDはこちらです。
                User ID: {{ev.Source.UserId}}
                """
        });
    }

    protected override async Task OnMessageAsync(MessageEvent ev)
    {
        //await Client.ReplyMessageAsync(ev.ReplyToken, $"あなたのユーザIDは \"{ev.Source.UserId}\" です。");
        if (MessageReceived != null)
        {
            await MessageReceived.Invoke(ev, Client);
        }
    }

}
