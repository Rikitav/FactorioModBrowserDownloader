using System.Windows;
using System.Windows.Controls;

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

        private void scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = (ScrollViewer)sender;
            MainWindowViewModel model = (MainWindowViewModel)DataContext;

            double delta = scroll.ScrollableHeight - scroll.ContentVerticalOffset;
            model.RequireListExtending = delta < 500;
        }
    }
}