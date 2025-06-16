using FactorioNexus.ApplicationPresentation.Extensions;
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

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            browser.ScrollChanged(sender, e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ViewModelBase model = (ViewModelBase)DataContext;
            model.ViewInitialized = true;
        }
    }
}