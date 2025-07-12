using FactorioNexus.ApplicationArchitecture.Requests;
using System.IO;
using System.Net.Http;

namespace FactorioNexus.ApplicationArchitecture.Dependencies
{
    public interface IFactorioNexusClient
    {
        public Task<TResponse> SendManagedRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class;
        public Task<HttpResponseMessage> SendMessageRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class;
        public Task<Stream> SendDataRequest(string requestUri, CancellationToken cancellationToken = default);
    }
}
