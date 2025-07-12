using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FactorioNexus.ApplicationArchitecture.Services
{
    public class StoringManager : DisposableBase<StoringManager>, IStoringManager
    {
        private readonly object StoreReadLook = new object();

        private ObservableCollection<ModStoreEntry> _storedMods = [];

        public ObservableCollection<ModStoreEntry> StoredMods => _storedMods;

        public async void ScanCurrentStorage(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Yield();
            lock (StoreReadLook)
            {
                StoredMods.Clear();
                DirectoryInfo storage = new DirectoryInfo(Path.Combine(App.Instance.Settings.GamedataDirectory, "Mods"));

                if (!storage.Exists)
                    return;

                foreach (DirectoryInfo modDir in storage.GetDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (modDir.Name == "__MACOSX")
                        continue;

                    TryAdd(modDir);
                }
            }
        }

        public bool TryAdd(DirectoryInfo directory)
        {
            lock (StoreReadLook)
            {
                try
                {
                    ModStoreEntry modStore = new ModStoreEntry(directory);
                    StoredMods.Add(modStore);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to add mod store {0}. {1}", [directory.Name, ex]);
                    return false;
                }
            }
        }

        public bool TryFind(string modId, [NotNullWhen(true)] out ModStoreEntry? result)
        {
            lock (StoreReadLook)
            {
                result = StoredMods.FirstOrDefault(store => store.Info.Name == modId);
                return result != null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_storedMods != null)
            {
                _storedMods.Clear();
                _storedMods = null!;
            }
        }
    }
}
