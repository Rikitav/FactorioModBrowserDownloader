using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.IO;

namespace FactorioNexus.Services
{
    public static class ModsStoringManager
    {
        public static readonly ObservableCollection<ModPageFullInfo> StoredMods = [];

        public static async void ScanCurrentStorage()
        {
            DirectoryInfo storage = new DirectoryInfo(Path.Combine(ApplicationSettingsManager.Current.GamedataDirectory, "Mods"));
            foreach (DirectoryInfo modDir in storage.GetDirectories())
            {

            }
        }
    }
}
