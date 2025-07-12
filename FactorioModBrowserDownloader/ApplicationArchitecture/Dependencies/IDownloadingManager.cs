using FactorioNexus.ApplicationArchitecture.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace FactorioNexus.ApplicationArchitecture.Dependencies
{
    public interface IDownloadingManager
    {
        public ObservableCollection<PackageDownloadEntry> DownloadingList { get; }

        public PackageDownloadEntry QueueModDownloading(ModEntryInfo modEntry, ReleaseInfo release, CancellationToken cancellationToken = default);
        public Task<PackageDownloadEntry> QueueDependencyDownloading(DependencyVersionRange dependency, CancellationToken cancellationToken = default);
        public bool TryFindEntry(string modId, [NotNullWhen(true)] out PackageDownloadEntry? result);
    }
}
