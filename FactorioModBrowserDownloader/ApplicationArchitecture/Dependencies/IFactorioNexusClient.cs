using FactorioNexus.ApplicationArchitecture.Requests;
using System.Net.Http;

namespace FactorioNexus.ApplicationArchitecture.Dependencies
{
    public interface IFactorioNexusClient
    {
        public Task<TResponse> RequestManaged<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class;
        public Task<HttpResponseMessage> Request<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class;
        public Task<HttpResponseMessage> Request(string requestUri, CancellationToken cancellationToken = default);
    }
}
