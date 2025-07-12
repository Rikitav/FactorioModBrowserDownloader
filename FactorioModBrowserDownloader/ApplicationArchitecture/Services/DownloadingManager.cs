using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class DownloadingManager(IFactorioNexusClient client, IDependencyResolver dependencyResolver, IStoringManager storingManager) : DisposableBase<DownloadingManager>, IDownloadingManager
    {
        private const int MaxDownloading = 5;
        //private static readonly string[] SkippingModsNames = ["base", "space-age", "quality"];

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

                entry = new ModDownloadEntry(this, DependencyResolver, modEntry, release);
                QueueDownloading(entry, cancellationToken);
                return entry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to queue {0} mod download. {1}", [modEntry.Id, ex]);
                throw;
            }
        }

        public async Task<PackageDownloadEntry> QueueDependencyDownloading(DependencyVersionRange dependency, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (TryFindEntry(dependency.ModId, out PackageDownloadEntry? entry))
                    return entry;

                entry = new DependencyDownloadEntry(dependency);
                await QueueDownloadingAsync(entry, cancellationToken);
                return entry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to queue {0} mod download. {1}", [dependency.ModId, ex]);
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
                Debug.WriteLine("Added {0} entry to downloading queue", [entry.ModId]);

                await _downloadingSemaphore.WaitAsync(cancellationToken);
                Debug.WriteLine("{0} downloading entry started", [entry.ModId]);

                DirectoryInfo? modDir = await entry.StartDownload(Client);
                if (modDir == null)
                {
                    Debug.WriteLine("Download entry \"{0}\" returned null directory. Considered failed to download", [entry.ModId]);
                    return;
                }

                StoringManager.TryAdd(modDir);
                Debug.WriteLine("{0} entry successfully downloaded", [entry.ModId]);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Cancelled {0} downloading entry", [entry.ModId]);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download {0} entry. {1}", [entry.ModId, ex]);
                throw;
            }
            finally
            {
                _downloadingSemaphore.Release();
                DownloadingList.Remove(entry);
                Debug.WriteLine("Removed {0} entry from downloading queue", [entry.ModId]);
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
