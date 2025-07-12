using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Services;
using FactorioNexus.PresentationFramework;
using FactorioNexus.PresentationFramework.Commands;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace FactorioNexus.ApplicationArchitecture.Models
{
    public enum ModStoreStatus
    {
        Queued,
        ResolvingDependencies,
        Downloading,
        Extracting,
        AwaitingDependencies,
        Downloaded,
        Canceled,
        Timeout,
        Faulted
    }

    public class DependencyDownloadEntry(DependencyVersionRange dependency) : PackageDownloadEntry(dependency.ModId)
    {
        private readonly DependencyVersionRange _dependencyInfo = dependency;

        protected override async Task<Stream> GetPackageStream(IFactorioNexusClient client, CancellationToken cancellationToken = default(CancellationToken))
            => await client.DownloadPackage(_dependencyInfo, cancellationToken);
    }

    public class ModDownloadEntry(IDownloadingManager downloadingManager, IDependencyResolver dependencyResolver, ModEntryInfo modEntry, ReleaseInfo release) : PackageDownloadEntry(modEntry.Id)
    {
        private readonly IDownloadingManager _downloadingManager = downloadingManager;
        private readonly IDependencyResolver _dependencyResolver = dependencyResolver;
        private readonly ModEntryInfo _modEntryFull = modEntry;
        private readonly ReleaseInfo _releaseInfo = release;

        protected override async Task<Stream> GetPackageStream(IFactorioNexusClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Task<PackageDownloadEntry>> downloadingTasks = [];
            List<string> dependenciesNames = [];

            Status = ModStoreStatus.ResolvingDependencies;
            foreach (DependencyVersionRange dependency in await _dependencyResolver.ResolveRequiredDependencies(_releaseInfo))
            {
                //DependencyDownloadEntry dependencyDownload = new DependencyDownloadEntry(dependency);
                dependenciesNames.Add(dependency.ModId);

                Task<PackageDownloadEntry> dependencyDownloadTask = _downloadingManager.QueueDependencyDownloading(dependency, CancellationToken.None);
                downloadingTasks.Add(dependencyDownloadTask);
            }

            Status = ModStoreStatus.Downloading;
            Stream pckgStream = await client.DownloadPackage(_modEntryFull, _releaseInfo, cancellationToken);
            Debug.WriteLine("Downloading \"{0}\" mod package with dependencies [{1}]", [ModId, string.Join(", ", dependenciesNames)]);

            Status = ModStoreStatus.AwaitingDependencies;
            await Task.WhenAll(downloadingTasks);
            Debug.WriteLine("Downloading session for mod \"{0}\" ended successfully", [ModId]);

            return pckgStream;
        }
    }

    public abstract class PackageDownloadEntry(string modId) : ViewModelBase
    {
        private readonly CancellCommand _cancellCommand = new CancellCommand();
        private readonly string _modId = modId;

        private ModStoreStatus _downloadingStatus = ModStoreStatus.Queued;
        private ModDownloadProgress _downloadingProgress = new ModDownloadProgress();
        private string? _errorMessage = null;
        private bool _working = false;

        public string ModId
        {
            get => _modId;
        }

        public ModStoreStatus Status
        {
            get => _downloadingStatus;
            protected set => Set(ref _downloadingStatus, value);
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
            get => _cancellCommand;
        }

        public async Task<DirectoryInfo?> StartDownload(IFactorioNexusClient client)
        {
            try
            {
                Working = true;

                Status = ModStoreStatus.Downloading;
                using Stream modArchiveStream = await DownloadPacakgeStream(client);

                Status = ModStoreStatus.Extracting;
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

                Debug.WriteLine("Failed to download mod {0}. {1}", [_modId, ex]);
                throw;
            }
            finally
            {
                Working = false;
            }
        }

        private async Task<Stream> DownloadPacakgeStream(IFactorioNexusClient client)
        {
            Stream modPackageStream = await GetPackageStream(client, _cancellCommand.Token);
            DownloadingProgress.Length = modPackageStream.Length;

            MemoryStream modArchiveStream = new MemoryStream();
            await modPackageStream.CopyToAsync(modArchiveStream, 1024, DownloadingProgress, _cancellCommand.Token);
            return modArchiveStream;
        }

        private string ExtractMemoryArchive(Stream modArchiveStream)
        {
            string extractTo = Path.Combine(App.Instance.Settings.GamedataDirectory, "Mods");

            using ZipArchive zipArchive = new ZipArchive(modArchiveStream);
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                string entryExtractTo = Path.Combine(extractTo, entry.FullName);
                _cancellCommand.Token.ThrowIfCancellationRequested();

                using FileStream entryFile = File.Open(entryExtractTo, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                using Stream entryStream = entry.Open();

                entryStream.CopyToAsync(entryFile, _cancellCommand.Token);
            }

            string firstEntry = zipArchive.Entries.ElementAt(0).FullName;
            string dirNname = firstEntry.Split('\\', '/').ElementAt(0);
            return Path.Combine(extractTo, dirNname);
        }

        protected abstract Task<Stream> GetPackageStream(IFactorioNexusClient client, CancellationToken cancellationToken = default);
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

        public override void OnPropertyChanged(string propertyName)
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
