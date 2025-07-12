using FactorioNexus.ApplicationArchitecture.Models;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.PresentationFramework.Controls
{
    public partial class ModPagePresenter : UserControl
    {
        public ModPagePresenter()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ModEntryFull modPage = (ModEntryFull)DataContext;
            if (modPage.DisplayLatestRelease.ModInfo.Dependencies == null || modPage.DisplayLatestRelease.ModInfo.Dependencies.Length == 0)
            {
                MessageBox.Show("This mod have no dependencies");
                return;
            }

            MessageBox.Show(string.Join("\n", modPage.DisplayLatestRelease.ModInfo.Dependencies.Select(dp => dp.ToString())));
        }
    }
}
