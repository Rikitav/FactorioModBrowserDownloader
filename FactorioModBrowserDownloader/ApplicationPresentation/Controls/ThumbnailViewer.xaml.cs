using FactorioNexus.ModPortal.Types;
using FactorioNexus.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    /// <summary>
    /// Логика взаимодействия для ThumbnailViewer.xaml
    /// </summary>
    public partial class ThumbnailViewer : UserControl
    {
        public bool IsDownloading
        {
            get => (bool)GetValue(IsDownloadingProperty);
            set => SetValue(IsDownloadingProperty, value);
        }

        public bool IsDownloadFaulted
        {
            get => (bool)GetValue(IsDownloadFaultedProperty);
            set => SetValue(IsDownloadFaultedProperty, value);
        }

        public bool IsThumbnailMissing
        {
            get => (bool)GetValue(IsThumbnailMissingProperty);
            set => SetValue(IsThumbnailMissingProperty, value);
        }

        public ThumbnailViewer()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.Property.Name)
            {
                case nameof(DataContext):
                    {
                        if (DataContext is not ModPageShortInfo modPage)
                            break;

                        //IsThumbnailMissing = true;
                        DownloadThumbnail(modPage);
                        break;
                    }
            }
        }

        private async void DownloadThumbnail(ModPageShortInfo modPage)
        {
            try
            {
                IsDownloading = true;
                await ModsThumbnailsManager.QueueThumbnailDownloading(modPage);
            }
            catch (MissingThumbnailException)
            {
                IsThumbnailMissing = true;
            }
            catch (FailedThumbnailException)
            {
                IsDownloadFaulted = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsDownloading = false;
            }
        }

        public static readonly DependencyProperty IsDownloadingProperty = DependencyProperty.Register(
            nameof(IsDownloading), typeof(bool), typeof(ThumbnailViewer),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsDownloadFaultedProperty = DependencyProperty.Register(
            nameof(IsDownloadFaulted), typeof(bool), typeof(ThumbnailViewer),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsThumbnailMissingProperty = DependencyProperty.Register(
            nameof(IsThumbnailMissing), typeof(bool), typeof(ThumbnailViewer),
            new PropertyMetadata(false));
    }
}
