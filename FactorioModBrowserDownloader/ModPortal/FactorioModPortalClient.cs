using FactorioModBrowserDownloader.Extensions;
using FactorioModBrowserDownloader.ModPortal.Types;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioModBrowserDownloader.ModPortal
{
    public class FactorioModPortalClient
    {
        private const string BaseApiUrl = "https://mods.factorio.com/api";
        private const string AssetsUrl = "https://assets-mod.factorio.com";
        private const int RetryCount = 5;
        private const int RetryThreshold = 60;

        private readonly CancellationToken GlobalCancelToken = default;
        private readonly HttpClient httpClient = new HttpClient();

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest;
        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived;

        public virtual async Task<TResponse> SendRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancelToken, cancellationToken);
            cancellationToken = cts.Token;

            Uri requestUri = new Uri(Path.Combine(BaseApiUrl, request.MethodName));
            using HttpContent? httpContent = request.ToHttpContent();

            HttpRequestMessage httpRequest = new HttpRequestMessage()
            {
                Method = request.HttpMethod,
                RequestUri = requestUri,
                Content = httpContent
            };

            for (int attempt = 1; attempt <= RetryCount; attempt++)
            {
                if (httpContent != null && RetryThreshold > 0 && RetryCount > 1 && !httpContent.Headers.ContentLength.HasValue)
                    await httpContent.LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);

                ApiRequestEventArgs? requestEventArgs = null;
                if (OnMakingApiRequest != null)
                {
                    requestEventArgs ??= new ApiRequestEventArgs(httpRequest);
                    await OnMakingApiRequest(this, requestEventArgs, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }

                using HttpResponseMessage httpResponse = await SendRequest(httpRequest, cancellationToken);
                if (OnApiResponseReceived != null)
                {
                    requestEventArgs ??= new ApiRequestEventArgs(httpRequest);
                    ApiResponseEventArgs args = new ApiResponseEventArgs(httpResponse, requestUri.AbsolutePath);
                    await OnApiResponseReceived(this, args, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new RequestException("Returned responce has negative status", httpResponse.StatusCode);

                TResponse? response = await DeserializeContent<TResponse>(httpResponse, cancellationToken).ConfigureAwait(false);
                return response ?? throw new RequestException("Responce is null", httpResponse.StatusCode);
            }

            throw new Exception("Out of request attempts");
        }

        public async Task<BitmapSource?> DownloadThumbnail(ModPageShortInfo modPage)
        {
            try
            {
                string thumbnailUrl = AssetsUrl + (modPage.Thumbnail ?? "/assets/.thumb.png/");
                Debug.WriteLine("Requesting thumbnail : " + thumbnailUrl);

                /*
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, thumbnailUrl);
                request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");
                */

                using (HttpResponseMessage response = await httpClient.GetAsync(thumbnailUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = new MemoryStream();
                        contentStream.CopyTo(bitmapImage.StreamSource);
                        bitmapImage.EndInit();

                        modPage.DownloadedThumbnail = bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load the image :", ex.Message);
            }

            return modPage.DownloadedThumbnail;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<T?> DeserializeContent<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            if (httpResponse.Content == null)
                throw new RequestException("Response doesn't contain any content", httpResponse.StatusCode);

            try
            {
                using Stream contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(contentStream, JsonClientAPI.Options, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception innerException)
            {
                throw new RequestException("There was an exception during deserialization of the response", httpResponse.StatusCode, innerException);
            }
        }

        private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
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
    }
}
