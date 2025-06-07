using System.Net.Http;

namespace FactorioModBrowserDownloader.ModPortal
{
    public class ApiRequestEventArgs(HttpRequestMessage? httpRequestMessage = null) : EventArgs()
    {
        public HttpRequestMessage? HttpRequestMessage { get; } = httpRequestMessage;
    }
}
