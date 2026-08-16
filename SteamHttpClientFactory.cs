using System.Net.Http;
using System.Net.Http.Headers;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// SteamKit HTTP policy proven on a physical iPhone during Step 05.
/// CM WebSocket requests must use SocketsHttpHandler; other SteamKit HTTP
/// purposes keep the platform-default handler.
/// </summary>
public static class SteamHttpClientFactory
{
    public static bool UsesSocketsHttpHandler(HttpClientPurpose purpose) =>
        purpose == HttpClientPurpose.CMWebSocket;

    public static HttpClient Create(HttpClientPurpose purpose)
    {
        var client = UsesSocketsHttpHandler(purpose)
            ? new HttpClient(new SocketsHttpHandler(), disposeHandler: true)
            : new HttpClient();

        var assemblyVersion = typeof(SteamConfiguration).Assembly
            .GetName().Version?.ToString(fieldCount: 3) ?? "UnknownVersion";

        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("SteamKit", assemblyVersion));

        return client;
    }
}
