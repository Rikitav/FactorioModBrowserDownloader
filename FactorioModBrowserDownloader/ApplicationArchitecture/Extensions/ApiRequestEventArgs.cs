using System.Net.Http;

namespace FactorioNexus.ApplicationArchitecture.Extensions
{
    public class ApiRequestEventArgs(HttpRequestMessage? httpRequestMessage = null) : EventArgs()
    {
        public HttpRequestMessage? HttpRequestMessage { get; } = httpRequestMessage;
    }
}
