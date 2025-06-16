using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace FactorioNexus.Services
{
    public enum ModStoreStatus
    {
        Ready,
        Queued,

        Downloading,
        Extracting,
        Downloaded,

        Canceled,
        Timeout,
        Faulted
    }

    public class ModDownloadEntry : ViewModelBase
    {
        private readonly CancellationTokenSource _cancellationSource;
        private readonly RelayCommand _cancellDownloadCommand;
        private readonly ModPageFullInfo _modPageFullInfo;
        private readonly ReleaseInfo _releaseInfo;

        private ModStoreStatus _downloadingStatus = ModStoreStatus.Ready;
        private ModDownloadProgress _downloadingProgress = new ModDownloadProgress();
        private string? _errorMessage = null;
        private bool _working = false;

        public string ModId
        {
            get => _modPageFullInfo.ModId;
        }

        public ModStoreStatus Status
        {
            get => _downloadingStatus;
            private set => Set(ref _downloadingStatus, value);
        }

        public ModDownloadProgress DownloadingProgress
        {
            get => _downloadingProgress;
            private set => Set(ref _downloadingProgress, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => Set(ref _errorMessage, value);
        }

        public bool Working
        {
            get => _working;
            private set => Set(ref _working, value);
        }

        public RelayCommand CancellDownloadCommand
        {
            get => _cancellDownloadCommand;
        }

        public ModDownloadEntry(ModPageFullInfo modPageFullInfo, ReleaseInfo release)
        {
            _cancellationSource = new CancellationTokenSource();
            _cancellDownloadCommand = new RelayCommand(_ => _cancellationSource.Cancel());
            _modPageFullInfo = modPageFullInfo;
            _releaseInfo = release;
            Status = ModStoreStatus.Queued;
        }

        public async Task<DirectoryInfo?> StartDownload()
        {
            try
            {
                Working = true;
                using Stream modArchiveStream = await DownloadPacakgeStream();
                string modDir = ExtractMemoryArchive(modArchiveStream);
                Status = ModStoreStatus.Downloaded;
                return new DirectoryInfo(modDir);
            }
            catch (TimeoutException)
            {
                Status = ModStoreStatus.Timeout;
                ErrorMessage = "Requesting timeout";
                return null;
            }
            catch (OperationCanceledException)
            {
                Status = ModStoreStatus.Canceled;
                ErrorMessage = "Download cancelled";
                return null;
            }
            catch (Exception ex)
            {
                Status = ModStoreStatus.Faulted;
                ErrorMessage = ex.Message;

                Debug.WriteLine("Failed to download mod {0}. {1}", [_modPageFullInfo.ModId, ex]);
                return null;
            }
            finally
            {
                Working = false;
            }
        }

        private async Task<Stream> DownloadPacakgeStream()
        {
            Status = ModStoreStatus.Downloading;
            Stream modPackageStream = await FactorioNexusClient.Instance.DownloadPackage(_modPageFullInfo, _releaseInfo, _cancellationSource.Token);
            DownloadingProgress.Length = modPackageStream.Length;

            MemoryStream modArchiveStream = new MemoryStream();
            await modPackageStream.CopyToAsync(modArchiveStream, 1024, DownloadingProgress, _cancellationSource.Token);
            return modArchiveStream;
        }

        private string ExtractMemoryArchive(Stream modArchiveStream)
        {
            Status = ModStoreStatus.Extracting;
            string extractTo = Path.Combine(ApplicationSettingsManager.Current.GamedataDirectory, "Mods");

            using ZipArchive zipArchive = new ZipArchive(modArchiveStream);
            zipArchive.ExtractToDirectory(extractTo, true);
            return Path.Combine(extractTo, zipArchive.Entries.ElementAt(0).Name);
        }
    }

    public class ModDownloadProgress : ViewModelBase, IProgress<long>
    {
        private long _length = 0;
        private long _downloaded = 0;
        private int _progress = 0;

        public long Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        public long Downloaded
        {
            get => _downloaded;
            set => Set(ref _downloaded, value);
        }

        public int Progress
        {
            get => _progress;
            private set => Set(ref _progress, value);
        }

        public void Report(long value)
        {
            Downloaded = value;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Downloaded):
                    Progress = (int)(Downloaded / Length * 100);
                    break;
            }
        }
    }
}
