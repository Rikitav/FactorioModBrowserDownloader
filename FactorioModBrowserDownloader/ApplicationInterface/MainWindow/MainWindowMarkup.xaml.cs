using FactorioNexus.PresentationFramework;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    }
}