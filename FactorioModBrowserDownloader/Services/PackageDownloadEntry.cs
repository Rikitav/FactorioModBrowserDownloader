using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace FactorioNexus.Services
{
    public enum ModStoreStatus
    {
        Queued,

        Downloading,
        Extracting,
        AwaitingDependencies,
        Downloaded,

        Canceled,
        Timeout,
        Faulted
    }

    public class DependencyDownloadEntry(DependencyInfo dependency) : PackageDownloadEntry()
    {
        private readonly DependencyInfo _dependencyInfo = dependency;
        public override string ModId => _dependencyInfo.ModId;

        protected override async Task<Stream> DownloadPackageInternal(CancellationToken cancellationToken = default(CancellationToken))
            => await FactorioNexusClient.Instance.DownloadPackage(_dependencyInfo, cancellationToken);
    }

    public class ModDownloadEntry(ModPageFullInfo modPageFullInfo, ReleaseInfo release) : PackageDownloadEntry()
    {
        private readonly ModPageFullInfo _modPageFullInfo = modPageFullInfo;
        private readonly ReleaseInfo _releaseInfo = release;
        public override string ModId => _modPageFullInfo.ModId;

        protected override async Task<Stream> DownloadPackageInternal(CancellationToken cancellationToken = default(CancellationToken))
            => await FactorioNexusClient.Instance.DownloadPackage(_modPageFullInfo, _releaseInfo, cancellationToken);

        public override async Task<DirectoryInfo?> StartDownload()
        {
            List<Task> downloadingTasks = [];
            foreach (DependencyInfo dependency in await ModsDownloadingManager.ScanRequiredDependencies(_releaseInfo))
            {
                DependencyDownloadEntry dependencyDownload = new DependencyDownloadEntry(dependency);
                Task dependencyDownloadTask = ModsDownloadingManager.QueuePackageDownloadingEntry(dependencyDownload);
                downloadingTasks.Add(dependencyDownloadTask);
            }

            Task<DirectoryInfo?> ModDownloadingTask = base.StartDownload();
            downloadingTasks.Add(ModDownloadingTask);
            await Task.WhenAll(downloadingTasks);
            return await ModDownloadingTask;
        }
    }

    public abstract class PackageDownloadEntry : ViewModelBase
    {
        private readonly CancellationTokenSource _cancellationSource;
        private readonly CancellCommand _cancellDownloadCommand;

        private ModStoreStatus _downloadingStatus = ModStoreStatus.Queued;
        private ModDownloadProgress _downloadingProgress = new ModDownloadProgress();
        private string? _errorMessage = null;
        private bool _working = false;

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

        public CancellCommand CancellDownloadCommand
        {
            get => _cancellDownloadCommand;
        }

        public abstract string ModId { get; }

        protected PackageDownloadEntry()
        {
            _cancellationSource = new CancellationTokenSource();
            _cancellDownloadCommand = new CancellCommand(_cancellationSource);
        }

        public virtual async Task<DirectoryInfo?> StartDownload()
        {
            try
            {
                Working = true;
                using Stream modArchiveStream = await DownloadPacakgeStream();
                string modDir = ExtractMemoryArchive(modArchiveStream);
                Status = ModStoreStatus.Downloaded;
                return new DirectoryInfo(modDir);
            }
            catch (OperationCanceledException)
            {
                Status = ModStoreStatus.Canceled;
                ErrorMessage = "Download canceled";
                return null;
            }
            catch (RequestException rex) when (rex.Aggreagate<TimeoutException>())
            {
                Status = ModStoreStatus.Timeout;
                ErrorMessage = "Requesting timeout";
                return null;
            }
            catch (RequestException rex)
            {
                Status = ModStoreStatus.Faulted;
                ErrorMessage = "Requesting error, " + rex.Message;
                throw;
            }
            catch (Exception ex)
            {
                Status = ModStoreStatus.Faulted;
                ErrorMessage = "Request faulted, " + ex.Message;

                Debug.WriteLine("Failed to download mod {0}. {1}", [ModId, ex]);
                throw;
            }
            finally
            {
                Working = false;
            }
        }

        private async Task<Stream> DownloadPacakgeStream()
        {
            Status = ModStoreStatus.Downloading;
            Stream modPackageStream = await DownloadPackageInternal(_cancellationSource.Token);
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

        protected abstract Task<Stream> DownloadPackageInternal(CancellationToken cancellationToken = default);
    }

    public class ModDownloadProgress : ViewModelBase, IProgress<long>
    {
        private double _length = 0;
        private double _downloaded = 0;
        private int _progress = 0;

        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        public double Downloaded
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
                    {
                        Progress = (int)(Downloaded / Length * 100);
                        break;
                    }
            }
        }
    }
}
