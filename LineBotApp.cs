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
        await ReplyIDAsync(ev);
    }

    protected async override Task OnFollowAsync(FollowEvent ev)
    {
        await ReplyIDAsync(ev);
    }

    protected override async Task OnMessageAsync(MessageEvent ev)
    {
        await Client.ReplyMessageAsync(ev.ReplyToken, $"あなたのユーザIDは \"{ev.Source.UserId}\" です。");
    }

    private async Task ReplyIDAsync(ReplyableEvent ev)
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
}