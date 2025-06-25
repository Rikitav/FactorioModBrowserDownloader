using FactorioNexus.ApplicationPresentation.Extensions;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Markups.ModsBrowser
{
    public partial class ModsBrowserPageMarkup : UserControl
    {
        public ModsBrowserPageMarkup()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ViewModelBase model = (ViewModelBase)DataContext;
            model.ViewInitialized = true;
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = (ScrollViewer)sender;
            ModsBrowserViewModel model = (ModsBrowserViewModel)DataContext;

            double delta = scroll.ScrollableHeight - scroll.ContentVerticalOffset;
            model.RequireListExtending = delta < 3000;
        }
    }
}
