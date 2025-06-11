using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ModPortal
{
    public class FactorioClient : IDisposable
    {
        private static FactorioClient? _singleton;

        public static FactorioClient Instance
        {
            get => _singleton ??= new FactorioClient();
        }

        private const string ApiUrl = "https://mods.factorio.com/api";
        private const string AssetsUrl = "https://assets-mod.factorio.com";
        private const string PackagesUrl = "https://mods-storage.re146.dev";
        private const int RetryCount = 5;
        private const int MaxThumbnailDownloading = 5;
        //private const int RetryThreshold = 60;

        private bool isDisposed = false;
        private HttpClient httpClient;
        private SemaphoreSlim thumbnailDownloadSemaphore;

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest;
        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived;

        private FactorioClient()
        {
            _singleton = this;
            httpClient = new HttpClient();
            thumbnailDownloadSemaphore = new SemaphoreSlim(MaxThumbnailDownloading);
        }

        public virtual async Task<TResponse> SendRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            StringBuilder uriBuilder = new StringBuilder(Path.Combine(ApiUrl, request.MethodName));
            uriBuilder.Append(request.ToUrlParameters());
            Uri requestUri = new Uri(uriBuilder.ToString());

            HttpRequestMessage httpRequest = new HttpRequestMessage()
            {
                Method = request.HttpMethod,
                RequestUri = requestUri
            };

            for (int attempt = 1; attempt <= RetryCount; attempt++)
            {
                /*
                if (httpContent != null && RetryThreshold > 0 && RetryCount > 1 && !httpContent.Headers.ContentLength.HasValue)
                    await httpContent.LoadIntoBufferAsync().ConfigureAwait(continueOnCapturedContext: false);
                */

                ApiRequestEventArgs? requestEventArgs = null;
                if (OnMakingApiRequest != null)
                {
                    requestEventArgs ??= new ApiRequestEventArgs(httpRequest);
                    await OnMakingApiRequest(this, requestEventArgs, cancellationToken).ConfigureAwait(false);
                }

                using HttpResponseMessage httpResponse = await SendRequest(httpRequest, cancellationToken);
                if (OnApiResponseReceived != null)
                {
                    requestEventArgs ??= new ApiRequestEventArgs(httpRequest);
                    ApiResponseEventArgs args = new ApiResponseEventArgs(httpResponse, requestUri.AbsolutePath);
                    await OnApiResponseReceived(this, args, cancellationToken).ConfigureAwait(false);
                }

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new RequestException("Returned responce has negative status", httpResponse.StatusCode);

                TResponse? response = await DeserializeContent<TResponse>(httpResponse, cancellationToken).ConfigureAwait(false);
                return response ?? throw new RequestException("Responce is null", httpResponse.StatusCode);
            }

            throw new Exception("Out of request attempts");
        }

        public async Task<BitmapSource> DownloadThumbnail(ModPageShortInfo modPage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                throw new ArgumentException("Cannot download thumbnail for mod page without thumbnail", nameof(modPage.Thumbnail));

            try
            {
                await thumbnailDownloadSemaphore.WaitAsync(cancellationToken);
                string thumbnailUrl = AssetsUrl + modPage.Thumbnail;
                Debug.WriteLine("Requesting thumbnail : {0}", thumbnailUrl);

                using (Stream contentStream = await SendDataRequest(thumbnailUrl, cancellationToken))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream();
                    contentStream.CopyTo(bitmapImage.StreamSource);
                    bitmapImage.EndInit();

                    thumbnailDownloadSemaphore.Release();
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download the thumbnail for {0}. {1}", modPage.ModId, ex);
                throw;
            }
        }

        public async Task DownloadPackage(ModPageEntryInfo modPage, ReleaseInfo releaseInfo)
        {
            string packageUri = PackagesUrl + string.Format("/{0}/{1}.zip", modPage.ModId, releaseInfo.Version);
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

        private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage httpRequest, CancellationToken cancellationToken = default)
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

        private async Task<Stream> SendDataRequest(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestUri)
            };

            using HttpResponseMessage responseMessage = await SendRequest(requestMessage, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null!;
            }

            if (thumbnailDownloadSemaphore != null)
            {
                thumbnailDownloadSemaphore.Dispose();
                thumbnailDownloadSemaphore = null!;
            }

            GC.SuppressFinalize(this);
            isDisposed = true;
        }
    }
}
