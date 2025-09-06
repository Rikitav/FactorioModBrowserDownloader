using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.PresentationFramework.Controls
{
    /// <summary>
    /// Логика взаимодействия для CurrentDownloadPresenter.xaml
    /// </summary>
    public partial class CurrentDownloadPresenter : UserControl
    {
        private readonly IDownloadingManager _downloadingManager;
        private readonly IFactorioNexusClient _nexusClient;

        public bool IsDownloading
        {
            get => (bool)GetValue(IsDownloadingProperty);
            set => SetValue(IsDownloadingProperty, value);
        }

        public PackageDownloadEntry? CurrentDownloading
        {
            get => (PackageDownloadEntry?)GetValue(CurrentDownloadingProperty);
            set => SetValue(CurrentDownloadingProperty, value);
        }

        public ModEntryFull? CurrentDownloadingEntry
        {
            get => (ModEntryFull?)GetValue(CurrentDownloadingEntryProperty);
            set => SetValue(CurrentDownloadingEntryProperty, value);
        }

        public CurrentDownloadPresenter()
        {
            InitializeComponent();

            _downloadingManager = App.Services.GetRequiredService<IDownloadingManager>();
            _nexusClient = App.Services.GetRequiredService<IFactorioNexusClient>();
            
            _downloadingManager.DownloadingList.CollectionChanged += CollectionChanged;
        }

        private async void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                CurrentDownloading = _downloadingManager.DownloadingList.FirstOrDefault();
                if (CurrentDownloading == null)
                {
                    CurrentDownloadingEntry = null;
                    IsDownloading = false;
                    return;
                }

                CurrentDownloadingEntry = await _nexusClient.FetchFullModInfo(CurrentDownloading.ModId);
                IsDownloading = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to resolve downloading entry. {0}", [ex]);
            }
        }

        public static readonly DependencyProperty IsDownloadingProperty = DependencyProperty.Register(
            nameof(IsDownloading), typeof(bool), typeof(CurrentDownloadPresenter),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CurrentDownloadingProperty = DependencyProperty.Register(
            nameof(CurrentDownloading), typeof(PackageDownloadEntry), typeof(CurrentDownloadPresenter),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CurrentDownloadingEntryProperty = DependencyProperty.Register(
            nameof(CurrentDownloadingEntry), typeof(ModEntryFull), typeof(CurrentDownloadPresenter),
            new FrameworkPropertyMetadata(null));
    }
}
