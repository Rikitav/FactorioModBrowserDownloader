using System.Net.Http;

namespace FactorioNexus.Infrastructure.Extensions
{
    public class ApiRequestEventArgs(HttpRequestMessage? httpRequestMessage = null) : EventArgs()
    {
        public HttpRequestMessage? HttpRequestMessage { get; } = httpRequestMessage;
    }
}
