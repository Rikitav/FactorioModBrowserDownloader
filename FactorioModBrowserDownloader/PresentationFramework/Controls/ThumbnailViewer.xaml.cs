﻿using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationArchitecture.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FactorioNexus.PresentationFramework.Controls
{
    public partial class ThumbnailViewer : UserControl
    {
        private readonly IThumbnailsResolver thumbnailsResolver;

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

        public BitmapSource DisplayThumbnail
        {
            get => (BitmapSource)GetValue(DisplayThumbnailProperty);
            set => SetValue(DisplayThumbnailProperty, value);
        }

        public ThumbnailViewer()
        {
            InitializeComponent();
            thumbnailsResolver = App.Instance.ServiceProvider.GetRequiredService<IThumbnailsResolver>();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.Property.Name)
            {
                case nameof(DataContext):
                    {
                        if (DataContext is not ModEntryShort modPage)
                            break;

                        DownloadThumbnail(modPage);
                        break;
                    }
            }
        }

        private async void DownloadThumbnail(ModEntryShort modPage)
        {
            try
            {
                IsDownloading = true;
                DisplayThumbnail = await thumbnailsResolver.ResolveThumbnail(modPage);
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
                IsDownloadFaulted = true;
                Debug.WriteLine(ex);
            }
            finally
            {
                IsDownloading = false;
            }
        }

        public static readonly DependencyProperty DisplayThumbnailProperty = DependencyProperty.Register(
            nameof(DisplayThumbnail), typeof(BitmapSource), typeof(ThumbnailViewer),
            new PropertyMetadata(null));

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
