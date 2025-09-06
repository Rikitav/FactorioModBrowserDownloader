using FactorioNexus.PresentationFramework;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FactorioNexus.ApplicationPresentation.Markups.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowMarkup : Window
    {
        public MainWindowMarkup()
        {
            InitializeComponent();
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            browser.ScrollChanged(sender, e);
        }

        public void Button_OpenDataDirectory_click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", App.Settings.GamedataDirectory);
        }

        private void Button_Header_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("");
        }
    }
}