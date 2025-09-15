using CommunityToolkit.Mvvm.ComponentModel;
using FactorioNexus.Infrastructure.Extensions;
using FactorioNexus.Infrastructure.Models;
using FactorioNexus.Infrastructure.Services;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.UserInterface.Extensions.Commands;
using FactorioNexus.Utilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
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

    public abstract partial class PackageDownloadEntry(ILogger<IDownloadingManager> logger, string modId) : ObservableObject
    {
        private readonly ILogger<IDownloadingManager> _logger = logger;

        [ObservableProperty]
        private string? _errorMessage = null;

        [ObservableProperty]
        private bool _working = false;

        [ObservableProperty]
        private ModStoreStatus _status = ModStoreStatus.Queued;

        public string ModId { get; } = modId;
        public CancellCommand CancellCommand { get; } = new CancellCommand();
        public ModDownloadProgress DownloadingProgress { get; } = new ModDownloadProgress();

        public async Task<DirectoryInfo?> StartDownload(IFactorioNexusClient client)
        {
            try
            {
                Working = true;
                Status = ModStoreStatus.Downloading;
                using Stream modArchiveStream = await DownloadPacakgeStream(client);

                Status = ModStoreStatus.Extracting;
                string modDir = await ExtractMemoryArchive(modArchiveStream, CancellCommand.Token);

                Status = ModStoreStatus.Downloaded;
                _logger.LogInformation("Downloading session for mod '{id}' ended successfully", ModId);
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
                _logger.LogError(rex, "mod request timeout '{id}'", ModId);
                return null;
            }
            catch (RequestException rex)
            {
                Status = ModStoreStatus.Faulted;
                ErrorMessage = "Requesting error, " + rex.Message;
                _logger.LogError(rex, "Failed to request mod '{id}'", ModId);
                throw;
            }
            catch (Exception ex)
            {
                Status = ModStoreStatus.Faulted;
                ErrorMessage = "Request faulted, " + ex.Message;
                _logger.LogError(ex, "Failed to download mod '{id}'", ModId);
                throw;
            }
            finally
            {
                Working = false;
            }
        }

        private async Task<Stream> DownloadPacakgeStream(IFactorioNexusClient client)
        {
            Stream modPackageStream = await GetPackageStream(client, CancellCommand.Token);
            DownloadingProgress.Length = modPackageStream.Length;

            MemoryStream modArchiveStream = new MemoryStream();
            await modPackageStream.CopyToAsync(modArchiveStream, 1024, DownloadingProgress, CancellCommand.Token);
            return modArchiveStream;
        }

        private static async Task<string> ExtractMemoryArchive(Stream modArchiveStream, CancellationToken cancellationToken = default)
        {
            string extractTo = Path.Combine(App.Settings.NormalizedGamedataDirectory, "Mods");
            using ZipArchive zipArchive = new ZipArchive(modArchiveStream);

            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                string entryExtractTo = Path.Combine(extractTo, entry.FullName.Replace("/", "\\"));
                cancellationToken.ThrowIfCancellationRequested();

                string? dir = Path.GetDirectoryName(entryExtractTo);
                if (string.IsNullOrEmpty(dir))
                    throw new InvalidDataException();

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using FileStream entryFile = File.Open(entryExtractTo, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                using Stream entryStream = entry.Open();
                await entryStream.CopyToAsync(entryFile, cancellationToken);
            }

            string firstEntry = zipArchive.Entries.ElementAt(0).FullName;
            string dirNname = firstEntry.Split('\\', '/').ElementAt(0);
            return Path.Combine(extractTo, dirNname);
        }

        protected abstract Task<Stream> GetPackageStream(IFactorioNexusClient client, CancellationToken cancellationToken = default);
    }

    public class ModDownloadEntry(ILogger<IDownloadingManager> logger, IDownloadingManager downloadingManager, IDependencyResolver dependencyResolver, ModEntryInfo modEntry, ReleaseInfo release) : PackageDownloadEntry(logger, modEntry.Id)
    {
        private readonly ILogger<IDownloadingManager> _logger = logger;
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
                Task<PackageDownloadEntry> dependencyDownloadTask = _downloadingManager.QueueDependencyDownloading(dependency, CancellationToken.None);
                downloadingTasks.Add(dependencyDownloadTask);
                dependenciesNames.Add(dependency.ModId);
            }

            Status = ModStoreStatus.Downloading;
            Stream pckgStream = await client.DownloadPackage(_modEntryFull, _releaseInfo, cancellationToken);
            _logger.LogInformation("Downloading \"{id}\" mod package with dependencies [{dependencies}]", ModId, string.Join(", ", dependenciesNames));

            Status = ModStoreStatus.AwaitingDependencies;
            await Task.WhenAll(downloadingTasks);
            return pckgStream;
        }
    }

    public class DependencyDownloadEntry(ILogger<IDownloadingManager> logger, DependencyVersionRange dependency) : PackageDownloadEntry(logger, dependency.ModId)
    {
        private readonly DependencyVersionRange _dependencyInfo = dependency;

        protected override async Task<Stream> GetPackageStream(IFactorioNexusClient client, CancellationToken cancellationToken = default(CancellationToken))
            => await client.DownloadPackage(_dependencyInfo, cancellationToken);
    }

    public partial class ModDownloadProgress : ObservableObject, IProgress<long>
    {
        [ObservableProperty]
        private double _length = 0;

        [ObservableProperty]
        private double _downloaded = 0;

        [ObservableProperty]
        private int _progress = 0;

        public void Report(long value)
        {
            Downloaded = value;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
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
