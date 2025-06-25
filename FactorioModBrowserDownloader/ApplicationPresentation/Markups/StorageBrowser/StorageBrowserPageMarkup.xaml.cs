using FactorioNexus.ApplicationPresentation.Extensions;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Markups.StorageBrowser
{
    public partial class StorageBrowserPageMarkup : UserControl
    {
        public StorageBrowserPageMarkup()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ViewModelBase model = (ViewModelBase)DataContext;
            model.ViewInitialized = true;
        }
    }
}
