using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Extensions;
using FactorioNexus.ApplicationArchitecture.Requests;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class FactorioNexusClient : DisposableBase<FactorioNexusClient>, IFactorioNexusClient
    {
        //private const int RetryThreshold = 60;
        private const int RetryCount = 3;

        private HttpClient httpClient;

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest;
        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived;

        public FactorioNexusClient()
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(1)
            };
        }

        public virtual async Task<TResponse> SendManagedRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            using HttpResponseMessage httpResponse = await SendMessageRequest(request, cancellationToken).ConfigureAwait(false);
            TResponse? response = await DeserializeContent<TResponse>(httpResponse, cancellationToken).ConfigureAwait(false);
            return response ?? throw new RequestException("Response is null", httpResponse.StatusCode);
        }

        public virtual async Task<HttpResponseMessage> SendMessageRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            using HttpRequestMessage httpRequest = request.ToRequestMessage();
            Debug.WriteLine("📤 Sending request on URI \"{0}\"", [httpRequest.RequestUri]);

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
                        throw new RequestException("Returned response has negative status", httpResponse.StatusCode);

                    return httpResponse;
                }
                catch (TimeoutException)
                {
                    continue;
                }
            }

            throw new Exception("Out of request attempts");
        }

        public async Task<Stream> SendDataRequest(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestUri)
            };

            HttpResponseMessage responseMessage = await SendRequestMessage(requestMessage, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<T?> DeserializeContent<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            if (httpResponse.Content == null)
                throw new RequestException("Response doesn't contain any content", httpResponse.StatusCode);

            try
            {
                using Stream contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(contentStream, Constants.JsonOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception innerException)
            {
                throw new RequestException("There was an exception during deserialization of the response", httpResponse.StatusCode, innerException);
            }
        }

        private async Task<HttpResponseMessage> SendRequestMessage(HttpRequestMessage httpRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                return await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (TaskCanceledException innerException)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw;

                throw new RequestException("Request timed out", innerException);
            }
            catch (Exception innerException2)
            {
                throw new RequestException("Exception during making request", innerException2);
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
