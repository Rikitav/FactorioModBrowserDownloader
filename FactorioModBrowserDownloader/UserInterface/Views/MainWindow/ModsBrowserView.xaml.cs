using FactorioNexus.UserInterface.ViewModels.Abstractions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactorioNexus.UserInterface.Views.MainWindow
{
    public partial class ModsBrowserView : UserControl
    {
        public ModsBrowserView()
        {
            InitializeComponent();
            //PreviewKeyDown += FocusChanger;
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroll = (ScrollViewer)sender;
            IModsBrowserViewModel model = (IModsBrowserViewModel)DataContext;

            double delta = scroll.ScrollableHeight - scroll.ContentVerticalOffset;
            model.RequireListExtending = delta < 3000;
        }

        private void CopyErrorMessage_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(errorTextBlock.Text);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            //Keyboard.Focus(searchBox);
        }
    }
}
