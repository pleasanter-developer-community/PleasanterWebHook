using LineDC.Messaging;
using LineDC.Messaging.Webhooks;
using LineDC.Messaging.Webhooks.Events;

namespace PleasanterWebHook;

class LineBotApp : WebhookApplication
{
    public LineBotApp(ILineMessagingClient client, string channelSecret)
        : base(client, channelSecret)
    { }

    protected async override Task OnJoinAsync(JoinEvent ev)
    {
        await Client.ReplyMessageAsync(ev.ReplyToken, GetJoinedMessage(ev.Source));
    }

    protected override async Task OnMessageAsync(MessageEvent ev)
    {
        await Client.ReplyMessageAsync(ev.ReplyToken, GetJoinedMessage(ev.Source));
    }

    private string GetJoinedMessage(WebhookEventSource source) =>　source.Type switch 
    { 
         EventSourceType.Group =>$$"""
            ようこそ！このグループのIDとあなたのIDはこちらです。
            Group ID: {{source.Id}}
            User ID: {{source.UserId}}
            """,
        EventSourceType.Room => $$"""
            ようこそ！このトークルームのIDとあなたのIDはこちらです。
            Room ID: {{source.Id}}
            User ID: {{source.UserId}}
            """,
        _=> $$"""
            ようこそ！あなたのIDはこちらです。
            User ID: {{source.UserId}}
            """
    };
}