using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;

namespace FactorioNexus.Infrastructure.Services
{
    public class StoringManager : DisposableBase<StoringManager>, IStoringManager
    {
        private readonly object StoreReadLook = new object();
        private readonly ILogger<StoringManager> _logger;

        private ObservableCollection<ModStoreEntry> _storedMods = [];

        public ObservableCollection<ModStoreEntry> StoredMods => _storedMods;
        public ILogger<StoringManager> Logger => _logger;

        public StoringManager(ILogger<StoringManager> logger)
        {
            _logger = logger;
            ScanCurrentStorage();
        }

        public async void ScanCurrentStorage(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Yield();
            Logger.LogTrace("Scanning mods storage");

            lock (StoreReadLook)
            {
                StoredMods.Clear();
                DirectoryInfo storage = new DirectoryInfo(Path.Combine(App.Settings.NormalizedGamedataDirectory, "Mods"));

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
                    Application.Current.Dispatcher.Invoke(() => StoredMods.Add(modStore));

                    Logger.LogTrace("Added mod '{id}'", modStore.Info.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to add mod store '{id}'.", directory.Name);
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
