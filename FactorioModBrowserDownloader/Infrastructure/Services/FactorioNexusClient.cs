using FactorioNexus.Infrastructure.Extensions;
using FactorioNexus.Infrastructure.Requests;
using FactorioNexus.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactorioNexus.Infrastructure.Services
{
    public class FactorioNexusClient : DisposableBase<FactorioNexusClient>, IFactorioNexusClient
    {
        private const int RetryCount = 3;

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly ILogger<FactorioNexusClient> _logger;

        private HttpClient httpClient;

        public ILogger<FactorioNexusClient> Logger => _logger;

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest;
        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived;

        public FactorioNexusClient(ILogger<FactorioNexusClient> logger)
        {
            _logger = logger;

            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(1)
            };
        }

        public virtual async Task<TResponse> RequestManaged<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            using HttpResponseMessage httpResponse = await Request(request, cancellationToken).ConfigureAwait(false);
            TResponse? response = await DeserializeContent<TResponse>(httpResponse, cancellationToken).ConfigureAwait(false);
            return response ?? throw new RequestException("Response is null", httpResponse.StatusCode);
        }

        public virtual async Task<HttpResponseMessage> Request<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            using HttpRequestMessage httpRequest = request.ToRequestMessage();
            Logger.LogTrace("Sending request on URI \"{url}\"", httpRequest.RequestUri);

            for (int attempt = 1; attempt <= RetryCount; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ApiRequestEventArgs? requestEventArgs = null;
                    if (OnMakingApiRequest != null)
                    {
                        requestEventArgs ??= new ApiRequestEventArgs(httpRequest);
                        await OnMakingApiRequest(this, requestEventArgs, cancellationToken).ConfigureAwait(false);
                    }

                    HttpResponseMessage httpResponse = await SendRequestMessage(httpRequest, cancellationToken);
                    if (OnApiResponseReceived != null)
                    {
                        requestEventArgs ??= new ApiRequestEventArgs(httpRequest);
                        ApiResponseEventArgs args = new ApiResponseEventArgs(httpResponse, httpRequest.RequestUri?.AbsolutePath);
                        await OnApiResponseReceived(this, args, cancellationToken).ConfigureAwait(false);
                    }

                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        //Logger.LogError("Request on URI \"{uri}\" return negative status code ({code})", httpRequest.RequestUri, httpResponse.StatusCode);
                        throw new RequestException("Returned response has negative status", httpResponse.StatusCode);
                    }

                    return httpResponse;
                }
                catch (TimeoutException)
                {
                    Logger.LogWarning("Request on URI \"{uri}\" timed out (Attempt : {attempt})", httpRequest.RequestUri, attempt);
                    continue;
                }
            }

            Logger.LogError("Request on URI \"{uri}\" ran out of request attempts", httpRequest.RequestUri);
            throw new Exception("Out of request attempts");
        }

        public async Task<HttpResponseMessage> Request(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestUri)
            };

            HttpResponseMessage responseMessage = await SendRequestMessage(requestMessage, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
            return responseMessage;
        }

        private async Task<T?> DeserializeContent<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            if (httpResponse.Content == null)
                throw new RequestException("Response without content", httpResponse.StatusCode);

            try
            {
                using Stream contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(contentStream, JsonOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception innerException)
            {
                Logger.LogError(innerException, "Failed to deserialize content of responce");
                throw new RequestException("There was an exception during deserialization of the response", httpResponse.StatusCode, innerException);
            }
        }

        private async Task<HttpResponseMessage> SendRequestMessage(HttpRequestMessage httpRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                return await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception innerException)
            {
                Logger.LogError(innerException, "Failed to send request on URI \"{uri}\"", httpRequest.RequestUri);
                throw new RequestException("Exception during making request", innerException);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null!;
            }
        }
    }

    public class RequestException : Exception
    {
        public readonly HttpStatusCode? HttpStatusCode;

        public RequestException(string message)
            : base(message) { }

        public RequestException(string message, Exception innerException)
            : base(message, innerException) { }

        public RequestException(string message, HttpStatusCode httpStatusCode)
            : base(message) => HttpStatusCode = httpStatusCode;

        public RequestException(string message, HttpStatusCode httpStatusCode, Exception? innerException)
            : base(message, innerException) => HttpStatusCode = httpStatusCode;
    }
}
