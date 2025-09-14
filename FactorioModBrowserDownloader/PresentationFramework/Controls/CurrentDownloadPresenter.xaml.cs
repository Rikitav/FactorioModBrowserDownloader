using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.PresentationFramework.Controls
{
    /// <summary>
    /// Логика взаимодействия для CurrentDownloadPresenter.xaml
    /// </summary>
    public partial class CurrentDownloadPresenter : UserControl
    {
        private readonly IDownloadingManager? _downloadingManager;
        private readonly IFactorioNexusClient? _nexusClient;
        private readonly ILogger<CurrentDownloadPresenter> _logger;

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

            if (App.Services != null)
            {
                _downloadingManager = App.Services.GetRequiredService<IDownloadingManager>();
                _nexusClient = App.Services.GetRequiredService<IFactorioNexusClient>();
                _logger = App.Services.GetRequiredService<ILogger<CurrentDownloadPresenter>>();
            }
            else
            {
                _logger = new NullLogger<CurrentDownloadPresenter>();
            }

            if (_downloadingManager != null)
            {
                _downloadingManager.DownloadingList.CollectionChanged += CollectionChanged;
            }
        }

        private async void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_downloadingManager == null)
                return;

            if (_nexusClient == null)
                return;

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
                _logger.LogError(ex, "Failed to resolve downloading entry. CurrentDownloading: '{id}'", CurrentDownloadingEntry?.Id ?? "NULL");
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
