using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class DownloadingManager(ILogger<DownloadingManager> logger, IFactorioNexusClient client, IDependencyResolver dependencyResolver, IStoringManager storingManager) : DisposableBase<DownloadingManager>, IDownloadingManager
    {
        private const int MaxDownloading = 5;

        private readonly ILogger<DownloadingManager> Logger = logger;
        private readonly IFactorioNexusClient Client = client;
        private readonly IDependencyResolver DependencyResolver = dependencyResolver;
        private readonly IStoringManager StoringManager = storingManager;

        private SemaphoreSlim _downloadingSemaphore = new SemaphoreSlim(MaxDownloading);
        private ObservableCollection<PackageDownloadEntry> _downloadingList = [];

        public ObservableCollection<PackageDownloadEntry> DownloadingList => _downloadingList;

        public PackageDownloadEntry QueueModDownloading(ModEntryInfo modEntry, ReleaseInfo release, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (TryFindEntry(modEntry.Id, out PackageDownloadEntry? entry))
                    return entry;

                entry = new ModDownloadEntry(Logger, this, DependencyResolver, modEntry, release);
                QueueDownloading(entry, cancellationToken);
                return entry;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to queue '{id}' mod download.", modEntry.Id);
                throw;
            }
        }

        public async Task<PackageDownloadEntry> QueueDependencyDownloading(DependencyVersionRange dependency, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (TryFindEntry(dependency.ModId, out PackageDownloadEntry? entry))
                    return entry;

                entry = new DependencyDownloadEntry(Logger, dependency);
                await QueueDownloadingAsync(entry, cancellationToken);
                return entry;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to queue '{id}' mod download.", dependency.ModId);
                throw;
            }
        }

        public async void QueueDownloading(PackageDownloadEntry entry, CancellationToken cancellationToken = default)
        {
            await QueueDownloadingAsync(entry, cancellationToken);
        }

        public async Task QueueDownloadingAsync(PackageDownloadEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                DownloadingList.Add(entry);
                Logger.LogTrace("Added '{id}' entry to downloading queue", entry.ModId);

                await _downloadingSemaphore.WaitAsync(cancellationToken);
                Logger.LogTrace("'{id}' downloading entry started", entry.ModId);

                DirectoryInfo? modDir = await entry.StartDownload(Client);
                if (modDir == null)
                {
                    Logger.LogWarning("Download entry '{id}' returned null directory. Considered failed to download", entry.ModId);
                    return;
                }

                StoringManager.TryAdd(modDir);
                Logger.LogInformation("'{id}' entry successfully downloaded", entry.ModId);
            }
            catch (OperationCanceledException)
            {
                Logger.LogTrace("Cancelled '{id}' downloading entry", entry.ModId);
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to download '{id}' entry.", entry.ModId);
                return;
            }
            finally
            {
                _downloadingSemaphore.Release();
                DownloadingList.Remove(entry);
                Logger.LogTrace("Removed '{id}' entry from downloading queue", entry.ModId);
            }
        }

        public bool TryFindEntry(string modId, [NotNullWhen(true)] out PackageDownloadEntry? result)
        {
            result = DownloadingList.FirstOrDefault(e => e.ModId == modId);
            return result != null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_downloadingList != null)
            {
                _downloadingList.Clear();
                _downloadingList = null!;
            }

            if (_downloadingSemaphore != null)
            {
                _downloadingSemaphore.Dispose();
                _downloadingSemaphore = null!;
            }
        }
    }
}
