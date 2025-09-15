using FactorioNexus.Infrastructure.Requests;
using System.Net.Http;

namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IFactorioNexusClient
    {
        public Task<TResponse> RequestManaged<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class;
        public Task<HttpResponseMessage> Request<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class;
        public Task<HttpResponseMessage> Request(string requestUri, CancellationToken cancellationToken = default);
    }
}
