using System.Net.Http;
using System.Net.Http.Headers;
using SteamKit2;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.7 HTTP client factory for SteamKit on iOS.
///
/// SteamKit 3.3.x routes CM WebSocket setup through SteamConfiguration.HttpClientFactory.
/// The native iOS NSUrlSessionHandler does not implement the synchronous HTTP send path
/// used by ClientWebSocket when an HttpMessageInvoker is supplied, so CM WebSocket setup
/// fails before ConnectedCallback. Use the fully-managed SocketsHttpHandler only for
/// HttpClientPurpose.CMWebSocket. Keep WebAPI/CDN on the platform default handler.
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
