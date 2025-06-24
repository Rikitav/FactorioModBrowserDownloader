using FactorioNexus.ModPortal.Types;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    /// <summary>
    /// Логика взаимодействия для ModPagePresenter.xaml
    /// </summary>
    public partial class ModPagePresenter : UserControl
    {
        public ModPagePresenter()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ModPageFullInfo modPage = (ModPageFullInfo)DataContext;
            MessageBox.Show(string.Join("\n", modPage.DisplayLatestRelease.ModInfo.Dependencies.Select(dp => dp.ToString())));
        }
    }
}
