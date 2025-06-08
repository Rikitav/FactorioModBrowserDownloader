using System.Net.Http;

namespace FactorioNexus.ModPortal
{
    public class ApiRequestEventArgs(HttpRequestMessage? httpRequestMessage = null) : EventArgs()
    {
        public HttpRequestMessage? HttpRequestMessage { get; } = httpRequestMessage;
    }
}
