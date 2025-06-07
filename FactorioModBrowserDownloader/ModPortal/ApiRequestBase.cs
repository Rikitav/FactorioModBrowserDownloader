using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FactorioModBrowserDownloader.ModPortal
{
    public abstract class ApiRequestBase<TResponse> where TResponse : class
    {
        public HttpMethod HttpMethod { get; private set; }
        public string MethodName { get; private set; }

        protected ApiRequestBase(HttpMethod httpMethod, params string[] methodPath)
        {
            HttpMethod = httpMethod;
            MethodName = Path.Combine(methodPath);
        }

        protected ApiRequestBase(params string[] methodPath)
        {
            HttpMethod = HttpMethod.Get;
            MethodName = Path.Combine(methodPath);
        }

        public virtual HttpContent? ToHttpContent()
        {
            return new StringContent(JsonSerializer.Serialize(this, GetType(), JsonClientAPI.Options), Encoding.UTF8, "application/json");
        }
    }
}
