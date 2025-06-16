using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;

namespace FactorioNexus.Services
{
    public enum EntryDownloadingStatus
    {
        Queued,
        Downloading,
        Canceled,
        Timeout,
        Faulted,
        Extracting,
        Done
    }

    public static class ModsDownloadingManager
    {
        private const int MaxDownloading = 5;
        private static readonly SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

        private static readonly ObservableCollection<DownloadingModEntry> DownloadingModsList = new ObservableCollection<DownloadingModEntry>();
        
        public static DownloadingModEntry QueueModDownloading(ModPageFullInfo modPage, ReleaseInfo release, CancellationToken cancellationToken = default)
        {
            try
            {
                DownloadingModEntry? entry = FindEntry(modPage);
                if (entry == null)
                {
                    entry = new DownloadingModEntry(modPage, release);
                    QueueModDownloadingEntry(entry, cancellationToken);
                }

                return entry;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download mod {0}. {1}", [modPage.ModId, ex]);
                throw;
            }
            finally
            {
                DownloadingSemaphore.Release();
            }
        }

        private static async void QueueModDownloadingEntry(DownloadingModEntry entry, CancellationToken cancellationToken = default)
        {
            DownloadingModsList.Add(entry);
            await DownloadingSemaphore.WaitAsync(cancellationToken);
            await entry.StartDownload();
            DownloadingModsList.Remove(entry);
        }

        public static DownloadingModEntry? FindEntry(ModPageFullInfo modPage)
        {
            return DownloadingModsList.FirstOrDefault(e => e.DownloadingId == modPage.ModId);
        }
    }

    public class DownloadingModEntry : ViewModelBase
    {
        private readonly CancellationTokenSource _cancellationSource;
        private readonly RelayCommand _cancellDownloadCommand;
        private readonly ModPageFullInfo _modPageFullInfo;
        private readonly ReleaseInfo _releaseInfo;

        private EntryDownloadingStatus _downloadingStatus;
        private long _downloadingLength = 0;
        private long _downloadingTotal = 0;
        private int _downloadingProgress = 0;
        private string? _errorMessage = null;
        private bool _working  = false;

        public string DownloadingId
        {
            get => _modPageFullInfo.ModId;
        }

        public EntryDownloadingStatus Status
        {
            get => _downloadingStatus;
            private set => Set(ref _downloadingStatus, value);
        }

        public long TotalLength
        {
            get => _downloadingLength;
            private set => Set(ref _downloadingLength, value);
        }

        public long TotalDownloaded
        {
            get => _downloadingTotal;
            private set => Set(ref _downloadingTotal, value);
        }

        public int Progress
        {
            get => _downloadingProgress;
            private set => Set(ref _downloadingProgress, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => Set(ref _errorMessage, value);
        }

        public bool Working
        {
            get => _working;
            private set => Set(ref _working, value);
        }

        public RelayCommand CancellDownloadCommand
        {
            get => _cancellDownloadCommand;
        }

        public DownloadingModEntry(ModPageFullInfo modPageFullInfo, ReleaseInfo release)
        {
            _cancellationSource = new CancellationTokenSource();
            _cancellDownloadCommand = new RelayCommand(_ => _cancellationSource.Cancel());
            _modPageFullInfo = modPageFullInfo;
            _releaseInfo = release;
            Status = EntryDownloadingStatus.Queued;
        }

        public async Task StartDownload()
        {
            try
            {
                Working = true;
                Status = EntryDownloadingStatus.Downloading;
                Stream modPackageStream = await FactorioNexusClient.Instance.DownloadPackage(_modPageFullInfo, _releaseInfo, _cancellationSource.Token);

                TotalLength = modPackageStream.Length;
                MemoryStream modArchiveStream = new MemoryStream();
                await modPackageStream.CopyToAsync(modArchiveStream, 1024, bytes => TotalDownloaded = bytes, _cancellationSource.Token);

                Status = EntryDownloadingStatus.Extracting;
                using ZipArchive zipArchive = new ZipArchive(modArchiveStream);
                string extractTo = Path.Combine(ApplicationSettingsManager.Current.GamedataDirectory, "Mods");

                zipArchive.ExtractToDirectory(extractTo, true);
                Status = EntryDownloadingStatus.Done;
            }
            catch (TimeoutException)
            {
                Status = EntryDownloadingStatus.Timeout;
                return;
            }
            catch (OperationCanceledException)
            {
                Status = EntryDownloadingStatus.Canceled;
                return;
            }
            catch (Exception ex)
            {
                Status = EntryDownloadingStatus.Faulted;
                ErrorMessage = ex.Message;
                Debug.WriteLine("Failed to download mod {0}. {1}", [_modPageFullInfo.ModId, ex]);
                return;
            }
            finally
            {
                Working = false;
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(TotalDownloaded):
                    Progress = (int)(TotalDownloaded / TotalLength * 100);
                    break;
            }
        }
    }
}
