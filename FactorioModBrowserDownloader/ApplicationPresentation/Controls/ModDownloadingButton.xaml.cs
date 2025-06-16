using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    /// <summary>
    /// Логика взаимодействия для ModDownloadingButton.xaml
    /// </summary>
    public partial class ModDownloadingButton : UserControl
    {
        public string DisplayString
        {
            get => (string)GetValue(DisplayStringProperty);
            set => SetValue(DisplayStringProperty, value);
        }

        public ModPageFullInfo ModPage
        {
            get => (ModPageFullInfo)GetValue(ModPageProperty);
            set => SetValue(ModPageProperty, value);
        }

        public DownloadingModEntry DownloadEntry
        {
            get => (DownloadingModEntry)GetValue(DownloadEntryProperty);
            set => SetValue(DownloadEntryProperty, value);
        }

        public ModDownloadingButton()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadEntry = ModsDownloadingManager.QueueModDownloading(ModPage, ModPage.DisplayRelease);
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
                        DownloadEntry = ModsDownloadingManager.FindEntry(ModPage);
                        break;
                    }

                    /*
                case nameof(DownloadEntry):
                    {
                        if (DownloadEntry != null)
                            DownloadEntry.PropertyChanged += DownloadEntryPropertyChanged;

                        break;
                    }
                    */
            }
        }

        /*
        private void DownloadEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DownloadingModEntry entry)
                return;

            switch (e.PropertyName)
            {
                case nameof(DownloadingModEntry.Status):
                    {
                        switch (entry.Status)
                        {
                            case EntryDownloadingStatus.Queued:
                                {
                                    DisplayString = "Queued";
                                    IsEnabled = false;
                                    break;
                                }

                            case EntryDownloadingStatus.Downloading:
                                {
                                    DisplayString = "Downloading...";
                                    IsEnabled = false;
                                    break;
                                }

                            case EntryDownloadingStatus.Extracting:
                                {
                                    DisplayString = "Extracting...";
                                    IsEnabled = false;
                                    break;
                                }

                            case EntryDownloadingStatus.Faulted:
                                {
                                    DisplayString = "Failed";
                                    IsEnabled = false;
                                    break;
                                }
                        }

                        break;
                    }
            }
        }
        */

        public static readonly DependencyProperty DisplayStringProperty = DependencyProperty.Register(
            nameof(DisplayString), typeof(string), typeof(ModDownloadingButton),
            new PropertyMetadata("Download"));

        public static readonly DependencyProperty ModPageProperty = DependencyProperty.Register(
            nameof(ModPage), typeof(ModPageFullInfo), typeof(ModDownloadingButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DownloadEntryProperty = DependencyProperty.Register(
            nameof(DownloadEntry), typeof(DownloadingModEntry), typeof(ModDownloadingButton),
            new PropertyMetadata(null));
    }
}
