using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FactorioNexus.Services
{
    public static class ModsStoringManager
    {
        private static readonly object StoreReadLook = new object();

        public static readonly ObservableCollection<ModStoreEntry> StoredMods = [];

        public static async void ScanCurrentStorage(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            lock (StoreReadLook)
            {
                StoredMods.Clear();
                DirectoryInfo storage = new DirectoryInfo(Path.Combine(ApplicationSettingsManager.Current.GamedataDirectory, "Mods"));

                if (!storage.Exists)
                    return;

                foreach (DirectoryInfo modDir in storage.GetDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TryAddModStore(modDir);
                }
            }
        }

        public static bool TryAddModStore(DirectoryInfo directory)
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

        public static bool TryFindStore(string modId, [NotNullWhen(true)] out ModStoreEntry? result)
        {
            lock (StoreReadLook)
            {
                result = StoredMods.FirstOrDefault(store => store.Info.Name == modId);
                return result != null;
            }
        }

        public static bool TryFindStore(ModPageFullInfo modPage, [NotNullWhen(true)] out ModStoreEntry? result)
            => TryFindStore(modPage.ModId, out result);

        public static bool TryFindStore(DependencyInfo dependency, [NotNullWhen(true)] out ModStoreEntry? result)
            => TryFindStore(dependency.ModId, out result);
    }
}
