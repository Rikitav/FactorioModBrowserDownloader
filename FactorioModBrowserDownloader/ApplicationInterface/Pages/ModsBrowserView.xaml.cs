using FactorioNexus.ApplicationInterface.Dependencies;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationInterface.Pages
{
    public partial class ModsBrowserView : UserControl
    {
        public ModsBrowserView()
        {
            InitializeComponent();
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = (ScrollViewer)sender;
            IModsBrowserViewModel model = (IModsBrowserViewModel)DataContext;

            double delta = scroll.ScrollableHeight - scroll.ContentVerticalOffset;
            model.RequireListExtending = delta < 3000;
        }

        private void CopyErrorMessage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(errorTextBlock.Text);
        }
    }
}
