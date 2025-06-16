using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Markups.ModsBrowser
{
    /// <summary>
    /// Логика взаимодействия для ModBrowserPageMarkup.xaml
    /// </summary>
    public partial class ModsBrowserPageMarkup : UserControl
    {
        public ModsBrowserPageMarkup()
        {
            InitializeComponent();
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = (ScrollViewer)sender;
            ModsBrowserViewModel model = (ModsBrowserViewModel)DataContext;

            double delta = scroll.ScrollableHeight - scroll.ContentVerticalOffset;
            model.RequireListExtending = delta < 500;
        }
    }
}
