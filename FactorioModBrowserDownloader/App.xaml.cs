using FactorioNexus.ApplicationPresentation.Markups.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;

namespace FactorioNexus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider _serviceProvider = default!;
        private static SettingsContainer _settings = default!;

        public static IServiceProvider Services => _serviceProvider;
        public static SettingsContainer Settings => _settings;
        public static string DataDirectory => Constants.PrivateAppDataDirectory;

        public App()
        {
            AttachConsoleTrace();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            _serviceProvider = new ServiceCollection().RegisterApplicationDefaults().BuildServiceProvider();
            _settings = SettingsContainer.LoadFromConfigFile();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = Services.GetRequiredService<MainWindowMarkup>();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (Services is IDisposable disposable)
                disposable.Dispose();
        }

        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!args.IsTerminating)
                return;

            if (args.ExceptionObject is not Exception exc)
                return;

            string msg = string.Format("\"{0}\" Application's execution was faulted by unhandled exception in {1} :\n\n{2}", AppDomain.CurrentDomain.FriendlyName, sender.ToString(), exc.ToString());
            MessageBox.Show(msg, AppDomain.CurrentDomain.FriendlyName, MessageBoxButton.OK);
        }

        private static void AttachConsoleTrace()
        {
#if DEBUG
            Console.Clear();
            Trace.Listeners.Add(new ConsoleTraceListener());
#endif
        }
    }
}
