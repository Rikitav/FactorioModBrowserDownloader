using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FactorioNexus.ApplicationPresentation.Markups.MainWindow
{
    public partial class MainWindowMarkup : Window
    {
        private static readonly Key CheatCodeActivatorKey = Key.Insert;
        private static readonly Key[] CheatCodeSequence = [Key.Up, Key.Up, Key.Down, Key.Down, Key.Left, Key.Right, Key.Left, Key.Right, Key.B, Key.A];
        private static readonly Key[] IgnoredTextFocusKeys = [Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.Tab, Key.Escape];

        private Queue<Key>? cheatCodeProgress = null;

        public MainWindowMarkup()
        {
            InitializeComponent();
            PreviewKeyDown += CheatCodeHandler;
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

        private void CheatCodeHandler(object sender, KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(CheatCodeActivatorKey) && e.Key != CheatCodeActivatorKey)
            {
                if (Keyboard.FocusedElement is TextBoxBase or PasswordBox)
                    return;

                if (IgnoredTextFocusKeys.Contains(e.Key))
                    return;

                Keyboard.Focus(browser);
                return;
            }

            cheatCodeProgress ??= new Queue<Key>(CheatCodeSequence);
            Key current = cheatCodeProgress.Peek();

            if (current != e.Key)
            {
                cheatCodeProgress = null;
                return;
            }

            cheatCodeProgress.Dequeue();
            if (cheatCodeProgress.Count == 0)
            {
                cheatCodeProgress = null;
                headerImage.Source = LoadCrack();
                return;
            }
        }

        private static BitmapImage LoadCrack()
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"pack://application:,,,/ApplicationInterface/MainWindow/cracktorio.png", UriKind.Absolute);
            bitmap.EndInit();
            return bitmap;
        }
    }
}