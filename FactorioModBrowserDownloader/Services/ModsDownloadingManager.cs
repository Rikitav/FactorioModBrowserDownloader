using FactorioNexus.ApplicationPresentation.Extensions;
using FactorioNexus.ModPortal;
using FactorioNexus.ModPortal.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace FactorioNexus.Services
{
    public enum EntryDownloadingStatus
    {
        Queued,
        Downloading,
        Canceled,
        Timeout,
        Faulted,
        Done
    }

    public static class ModsDownloadingManager
    {
        private const int MaxDownloading = 5;
        private static readonly SemaphoreSlim DownloadingSemaphore = new SemaphoreSlim(MaxDownloading);

        private static readonly ObservableCollection<DownloadingModEntry> DownloadingModsList = new ObservableCollection<DownloadingModEntry>();
        
        public static async Task QueueModDownloading(ModPageFullInfo modPage, ReleaseInfo? release = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await DownloadingSemaphore.WaitAsync(cancellationToken);
                DownloadingModEntry entry = new DownloadingModEntry(modPage);
                
                DownloadingModsList.Add(entry);
                using Stream modArchiveStream = await entry.StartDownload(release ?? modPage.DisplayRelease);
           
                using ZipArchive zipArchive = new ZipArchive(modArchiveStream);

                string extractTo = Path.Combine();
                zipArchive.ExtractToDirectory(extractTo, true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download mod");
            }
            finally
            {
                DownloadingSemaphore.Release();
            }
        }
    }

    public class DownloadingModEntry : ViewModelBase
    {
        private readonly CancellationTokenSource _cancellationSource;
        private readonly RelayCommand _cancellDownloadCommand;
        private readonly ModPageFullInfo _modPageFullInfo;

        private EntryDownloadingStatus _downloadingStatus;
        private long _downloadingLength = 0;
        private long _downloadingTotal = 0;
        private int _downloadingProgress = 0;

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

        public RelayCommand CancellDownloadCommand
        {
            get => _cancellDownloadCommand;
        }

        public DownloadingModEntry(ModPageFullInfo modPageFullInfo)
        {
            _cancellationSource = new CancellationTokenSource();
            _cancellDownloadCommand = new RelayCommand(_ => _cancellationSource.Cancel());
            _modPageFullInfo = modPageFullInfo;
            Status = EntryDownloadingStatus.Queued;
        }

        public async Task<Stream> StartDownload(ReleaseInfo release)
        {
            try
            {
                Status = EntryDownloadingStatus.Downloading;
                Stream stream = await FactorioNexusClient.Instance.DownloadPackage(_modPageFullInfo, release, _cancellationSource.Token);
                TotalLength = stream.Length;

                MemoryStream finalStream = new MemoryStream();
                await stream.CopyToAsync(finalStream, 1024, bytes => TotalDownloaded = bytes, _cancellationSource.Token);
               
                Status = EntryDownloadingStatus.Done;
                return finalStream;
            }
            catch (TimeoutException)
            {
                Status = EntryDownloadingStatus.Timeout;
                return Stream.Null;
            }
            catch (OperationCanceledException)
            {
                Status = EntryDownloadingStatus.Canceled;
                return Stream.Null;
            }
            catch (Exception ex)
            {
                Status = EntryDownloadingStatus.Faulted;
                Debug.WriteLine("Failed to download mod {0}. {1}", _modPageFullInfo.ModId, ex);
                return Stream.Null;
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
