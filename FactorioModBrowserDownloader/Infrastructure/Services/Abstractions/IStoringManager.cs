using FactorioNexus.ApplicationArchitecture.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FactorioNexus.Infrastructure.Services.Abstractions
{
    public interface IStoringManager
    {
        public ObservableCollection<ModStoreEntry> StoredMods { get; }

        public void ScanCurrentStorage(CancellationToken cancellationToken = default);
        public bool TryAdd(DirectoryInfo directory);
        public bool TryFind(string modId, [NotNullWhen(true)] out ModStoreEntry? result);
    }
}
