using System.Net.Http;

namespace FactorioNexus.Infrastructure.Extensions
{
    public class ApiResponseEventArgs(HttpResponseMessage responseMessage, string? apiRequestPath) : EventArgs()
    {
        public HttpResponseMessage ResponseMessage { get; } = responseMessage;
        public string? ApiRequestPath { get; } = apiRequestPath;
    }
}
