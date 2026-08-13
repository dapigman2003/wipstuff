using System.Net.Http;
using System.Net.Http.Headers;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.12 retained HTTP client factory for SteamKit on iOS.
///
/// Step 05.7 proved that the platform-default NSUrlSessionHandler cannot service
/// SteamKit's CM WebSocket custom-invoker path on iOS. Step 05.8 then proved that
/// SocketsHttpHandler works for both HTTPS and ClientWebSocket on the physical iPhone.
/// Keep SocketsHttpHandler only for HttpClientPurpose.CMWebSocket; WebAPI/CDN remain
/// on the platform-default HttpClient.
/// </summary>
public static class SteamHttpClientFactory
{
    public static HttpClient Create(HttpClientPurpose purpose)
    {
        HttpClient client;

        if (purpose == HttpClientPurpose.CMWebSocket)
        {
            var handler = new SocketsHttpHandler();
            client = new HttpClient(handler, disposeHandler: true);
        }
        else
        {
            client = new HttpClient();
        }

        var assemblyVersion = typeof(SteamConfiguration).Assembly
            .GetName().Version?.ToString(fieldCount: 3) ?? "UnknownVersion";
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("SteamKit", assemblyVersion));

        return client;
    }
}
