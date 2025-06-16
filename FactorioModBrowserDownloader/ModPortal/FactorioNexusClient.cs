using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal.Types;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ModPortal
{
    public class FactorioNexusClient : IDisposable
    {
        private static FactorioNexusClient? SingletonInstance;
        
        public static FactorioNexusClient Instance
        {
            get => SingletonInstance ??= new FactorioNexusClient();
        }

        private const string ApiUrl = "https://mods.factorio.com/api";
        private const string AssetsUrl = "https://assets-mod.factorio.com";
        private const string PackagesUrl = "https://mods-storage.re146.dev";
        private const int RetryCount = 3;
        //private const int RetryThreshold = 60;

        private bool isDisposed = false;
        private HttpClient httpClient;

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest;
        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived;

        private FactorioNexusClient()
        {
            httpClient = new HttpClient();
        }

        public virtual async Task<TResponse> SendRequest<TResponse>(ApiRequestBase<TResponse> request, CancellationToken cancellationToken = default(CancellationToken)) where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            StringBuilder uriBuilder = new StringBuilder();
            uriBuilder.Append(Path.Combine(ApiUrl, request.MethodName));
            request.BuildParameters(uriBuilder);

            Uri requestUri = new Uri(uriBuilder.ToString());

            HttpRequestMessage httpRequest = new HttpRequestMessage()
            {
                Method = request.HttpMethod,
                RequestUri = requestUri
            };

            for (int attempt = 1; attempt <= RetryCount; attempt++)
            {
                try
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
                catch (TimeoutException)
                {
                    continue;
                }
            }

            throw new Exception("Out of request attempts");
        }

        public async Task<BitmapSource> DownloadThumbnail(ModPageShortInfo modPage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modPage.Thumbnail))
                throw new ArgumentException("Cannot download thumbnail for mod page without thumbnail", nameof(modPage));

            try
            {
                string thumbnailUrl = AssetsUrl + modPage.Thumbnail;
                Debug.WriteLine("Requesting thumbnail : {0}", thumbnailUrl);

                using (Stream contentStream = await SendDataRequest(thumbnailUrl, cancellationToken))
                {
                    BitmapImage bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream();
                    contentStream.CopyTo(bitmapImage.StreamSource);
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download the thumbnail for {0}. {1}", [modPage.ModId, ex]);
                throw;
            }
        }

        public async Task<Stream> DownloadPackage(ModPageEntryInfo modPage, ReleaseInfo releaseInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                string packageUri = PackagesUrl + string.Format("/{0}/{1}.zip", modPage.ModId, releaseInfo.Version);
                Debug.WriteLine("Requesting package : {0}", packageUri);
                return await SendDataRequest(packageUri, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download the thumbnail for {0}. {1}", [modPage.ModId, ex]);
                throw;
            }
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
            /*
            if (!NativeMethods.IsInternetConnectionAvailable())
                throw new RequestException("No internet connection");
            */

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

            HttpResponseMessage responseMessage = await SendRequest(requestMessage, cancellationToken);
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

            GC.SuppressFinalize(this);
            isDisposed = true;
        }

        /*
        private static class NativeMethods
        {
            public enum InternetConnectionState
            {
                CONFIGURED = 0x40,
                LAN = 0x02,
                MODEM = 0x01,
                MODEM_BUSY = 0x08,
                OFFLINE = 0x20,
                PROXY = 0x04
            }

            private const string _CheckUriString = @"https://sourceforge.net/projects/refind";
            public const int ERROR_NOT_CONNECTED = 0x8CA;

            [DllImport("wininet.dll", SetLastError = true)]
            public extern static bool InternetGetConnectedState(out InternetConnectionState lpdwFlags, int dwReserved);

            [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            public extern static bool InternetCheckConnectionA(string lpszUrl, int dwFlags, int dwReserved);

            public static bool IsInternetConnectionAvailable()
            {
                // Checking for any internet devices is active
                if (!InternetGetConnectedState(out InternetConnectionState state, 0))
                {
                    Debug.WriteLine("No internet devices online, state : {0}", [state]);
                    return false;
                }

                // Checking for server availablity
                if (!InternetCheckConnectionA(_CheckUriString, 0x00000001, 0))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    Debug.WriteLine(lastError == ERROR_NOT_CONNECTED ? "No internet connection" : "Server unavailable");
                    return false;
                }

                return true;
            }
        }
        */
    }
}
