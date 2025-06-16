using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace FactorioNexus.Services
{
    public static class ModsStoringManager
    {
        public static readonly ObservableCollection<ModStoreEntry> StoredMods = [];

        public static async void ScanCurrentStorage(CancellationToken cancellationToken = default)
        {
            StoredMods.Clear();
            DirectoryInfo storage = new DirectoryInfo(Path.Combine(ApplicationSettingsManager.Current.GamedataDirectory, "Mods"));

            foreach (DirectoryInfo modDir in storage.GetDirectories())
            {
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
                TryAddModStore(modDir);
            }
        }

        public static bool TryAddModStore(DirectoryInfo directory)
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
}
