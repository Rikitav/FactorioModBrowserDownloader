using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    public partial class ModDownloadingButton : Button
    {
        public string DisplayString
        {
            get => (string)GetValue(DisplayStringProperty);
            set => SetValue(DisplayStringProperty, value);
        }

        public DrawingImage DisplayImage
        {
            get => (DrawingImage)GetValue(DisplayImageProperty);
            set => SetValue(DisplayImageProperty, value);
        }

        public ModPageFullInfo ModPage
        {
            get => (ModPageFullInfo)GetValue(ModPageProperty);
            set => SetValue(ModPageProperty, value);
        }

        public PackageDownloadEntry DownloadEntry
        {
            get => (PackageDownloadEntry)GetValue(DownloadEntryProperty);
            set => SetValue(DownloadEntryProperty, value);
        }

        public bool IsDownloading
        {
            get => (bool)GetValue(IsDownloadingProperty);
            set => SetValue(IsDownloadingProperty, value);
        }

        public bool IsAwaitingDependencies
        {
            get => (bool)GetValue(IsAwaitingDependenciesProperty);
            set => SetValue(IsAwaitingDependenciesProperty, value);
        }

        public bool IsExtracting
        {
            get => (bool)GetValue(IsExtractingProperty);
            set => SetValue(IsExtractingProperty, value);
        }

        public bool IsFaulted
        {
            get => (bool)GetValue(IsFaultedProperty);
            set => SetValue(IsFaultedProperty, value);
        }

        public bool IsCanceled
        {
            get => (bool)GetValue(IsCanceledProperty);
            set => SetValue(IsCanceledProperty, value);
        }

        public bool IsDownloaded
        {
            get => (bool)GetValue(IsDownloadedProperty);
            set => SetValue(IsDownloadedProperty, value);
        }

        public bool HasUpdate
        {
            get => (bool)GetValue(HasUpdateProperty);
            set => SetValue(HasUpdateProperty, value);
        }

        public string FaultReason
        {
            get => (string)GetValue(FaultReasonProperty);
            set => SetValue(FaultReasonProperty, value);
        }

        public int DownloadProgress
        {
            get => (int)GetValue(DownloadProgressProperty);
            set => SetValue(DownloadProgressProperty, value);
        }

        public ModDownloadingButton()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsDownloaded)
                    return;

                IsFaulted = false;
                IsCanceled = false;
                IsDownloaded = false;
                FaultReason = string.Empty;

                DownloadEntry = ModsDownloadingManager.QueueModDownloading(ModPage, ModPage.DisplayLatestRelease);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download mod {0}. {1}", [ModPage.ModId, ex]);
                MessageBox.Show(string.Format("Failed to download mod {0}. {1}", [ModPage.ModId, ex]));
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.Property.Name)
            {
                case nameof(ModPage):
                    {
                        if (ModsStoringManager.TryFindStore(ModPage, out ModStoreEntry? store))
                        {
                            if (store.Info.ModVersion != null && ModPage.DisplayLatestRelease.Version > store.Info.ModVersion)
                            {
                                HasUpdate = true;
                                return;
                            }

                            IsDownloaded = true;
                            return;
                        }
                        
                        if (ModsDownloadingManager.TryFindEntry(ModPage, out PackageDownloadEntry? entry))
                        {
                            DownloadEntry = entry;
                            return;
                        }

                        break;
                    }

                case nameof(DownloadEntry):
                    {
                        IsDownloading = true;
                        DownloadEntry.PropertyChanged += DownloadStatusChanged;
                        DownloadEntry.DownloadingProgress.PropertyChanged += DownloadProgressChanged;
                        break;
                    }
            }
        }

        private void DownloadStatusChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PackageDownloadEntry entry)
                return;

            if (e.PropertyName != nameof(PackageDownloadEntry.Status))
                return;

            switch (entry.Status)
            {
                case ModStoreStatus.Downloading:
                    IsDownloading = true;
                    IsDownloaded = false;
                    break;

                case ModStoreStatus.Extracting:
                    IsDownloading = false;
                    IsExtracting = true;
                    break;

                case ModStoreStatus.Faulted:
                    IsDownloading = false;
                    FaultReason = entry.ErrorMessage ?? string.Empty;
                    IsFaulted = true;
                    break;

                case ModStoreStatus.Timeout:
                    IsDownloading = false;
                    FaultReason = "Timed out";
                    IsFaulted = true;
                    break;
                
                case ModStoreStatus.Canceled:
                    IsDownloading = false;
                    break;

                case ModStoreStatus.Downloaded:
                    IsDownloading = false;
                    IsDownloaded = true;
                    break;
            }
        }

        private void DownloadProgressChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ModDownloadProgress progress)
                return;

            DownloadProgress = progress.Progress;
        }

        public static readonly DependencyProperty DisplayStringProperty = DependencyProperty.Register(
            nameof(DisplayString), typeof(string), typeof(ModDownloadingButton),
            new PropertyMetadata("Download"));

        public static readonly DependencyProperty DisplayImageProperty = DependencyProperty.Register(
            nameof(DisplayImage), typeof(DrawingImage), typeof(ModDownloadingButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ModPageProperty = DependencyProperty.Register(
            nameof(ModPage), typeof(ModPageFullInfo), typeof(ModDownloadingButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DownloadEntryProperty = DependencyProperty.Register(
            nameof(DownloadEntry), typeof(PackageDownloadEntry), typeof(ModDownloadingButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsDownloadingProperty = DependencyProperty.Register(
            nameof(IsDownloading), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsAwaitingDependenciesProperty = DependencyProperty.Register(
            nameof(IsAwaitingDependencies), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsExtractingProperty = DependencyProperty.Register(
            nameof(IsExtracting), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsFaultedProperty = DependencyProperty.Register(
            nameof(IsFaulted), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsCanceledProperty = DependencyProperty.Register(
            nameof(IsCanceled), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsDownloadedProperty = DependencyProperty.Register(
            nameof(IsDownloaded), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty HasUpdateProperty = DependencyProperty.Register(
            nameof(HasUpdate), typeof(bool), typeof(ModDownloadingButton),
            new PropertyMetadata(false));

        public static readonly DependencyProperty FaultReasonProperty = DependencyProperty.Register(
            nameof(FaultReason), typeof(string), typeof(ModDownloadingButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DownloadProgressProperty = DependencyProperty.Register(
            nameof(DownloadProgress), typeof(int), typeof(ModDownloadingButton),
            new PropertyMetadata(0));
    }
}
