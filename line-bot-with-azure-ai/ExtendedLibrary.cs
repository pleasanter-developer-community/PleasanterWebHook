using LineDC.Messaging;
using PleasanterWebHook;
using PleasanterWebHook.Controller;
using PleasanterWebHook.Model;

namespace Implem.Pleasanter.NetCore.ExtendedLibrary;

public static class ExtendedLibrary
{
    public static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationRoot config = LoadConfiguration();
        var settings = config.Get<WebhookSettings>()
            ?? throw new InvalidOperationException("WebhookSettings could not be loaded from configuration.");

        services
            .AddHttpClient<ILineBotApp, LineBotApp>(httpClient =>
            {
                var client = LineMessagingClient.Create(httpClient, settings.LineChannelAccessToken);
                return new LineBotApp(client, settings.LineChannelSecret);
            });

        services
            .AddScoped(provider => KernelManager.CreateKernel(settings));
    }

    private static IConfigurationRoot LoadConfiguration()
    {
        var dir = Path.GetDirectoryName(typeof(WebHookController).Assembly.Location)
            ?? AppContext.BaseDirectory;
        var netcoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "";
        var settingsPath = Path.Combine(dir, "webhooksettings.json");
        var devSettingsPath = Path.Combine(dir, $"webhooksettings.{netcoreEnv}.json");
        var confBuilder = new ConfigurationBuilder();
        if (System.IO.File.Exists(settingsPath))
        {
            confBuilder.AddJsonFile(settingsPath);
        }
        if (System.IO.File.Exists(devSettingsPath))
        {
            confBuilder.AddJsonFile(devSettingsPath);
        }
        return confBuilder.Build();
    }
}
