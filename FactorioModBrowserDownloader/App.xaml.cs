using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Services;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.ApplicationInterface.ViewModels;
using FactorioNexus.ApplicationPresentation.Markups.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FactorioNexus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string DataDirectory => Constants.PrivateAppDataDirectory;
        public static IServiceProvider Services { get; private set; } = default!;
        public static SettingsContainer Settings { get; private set; } = new SettingsContainer();

        public App()
        {
            Directory.CreateDirectory(DataDirectory);
            Settings = SettingsContainer.LoadFromConfigFile();

            AttachConsoleTrace();
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindowMarkup mainWindow = Services.GetRequiredService<MainWindowMarkup>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (Services is IDisposable disposable)
                disposable.Dispose();
        }

        protected virtual void AttachConsoleTrace()
        {
#if DEBUG
            Console.Clear();
            Trace.Listeners.Add(new ConsoleTraceListener());
#endif
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Configure Logging
            services.AddLogging();

            // Register Services
            services.AddSingleton<IDatabaseIndexer, DatabaseIndexer>();
            services.AddSingleton<IDependencyResolver, DependencyResolver>();
            services.AddSingleton<IDownloadingManager, DownloadingManager>();
            services.AddSingleton<IFactorioNexusClient, FactorioNexusClient>();
            services.AddSingleton<IStoringManager, StoringManager>();
            services.AddSingleton<IThumbnailsResolver, ThumbnailsResolver>();

            // Register ViewModels
            if (IsDesign())
            {
                services.AddSingleton<IModsBrowserViewModel, ModsBrowserViewModelMockup>();
                services.AddSingleton<IModsStorageViewModel, ModsStorageViewModelMockup>();
                services.AddSingleton<IApplicationSettingViewModel, ApplicationSettingsViewModelMockup>();
            }
            else
            {
                services.AddSingleton<IModsBrowserViewModel, ModsBrowserViewModel>();
                services.AddSingleton<IModsStorageViewModel, ModsStorageViewModel>();
                services.AddSingleton<IApplicationSettingViewModel, ApplicationSettingsViewModel>();
            }

            services.AddDbContext<IndexedModPortalDatabase>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

            // Register Views
            services.AddSingleton<MainWindowMarkup>();
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

        private static bool IsDesign()
        {
            DependencyObject dummyObject = new DependencyObject();
            return DesignerProperties.GetIsInDesignMode(dummyObject);
        }

        /*
        private static partial class NativeMethods
        {
            public enum InternetConnectionState
            {
                CONFIGURED = 0x40,
                LAN = 0x02,
                MODEM = 0x01,
                MODEM_BUSY = 0x08,
                OFFLINE = 0x20,
                PROXY = 0x04
            }

            private const string _CheckUriString = @"https://mods.factorio.com";
            public const int ERROR_NOT_CONNECTED = 0x8CA;

            [LibraryImport("wininet.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool InternetGetConnectedState(out InternetConnectionState lpdwFlags, int dwReserved);

            [LibraryImport("wininet.dll", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool InternetCheckConnectionA(string lpszUrl, int dwFlags, int dwReserved);

            public static bool IsInternetConnectionAvailable()
            {
                // Checking for any Internet devices is active
                if (!InternetGetConnectedState(out InternetConnectionState state, 0))
                {
                    Debug.WriteLine("No internet devices online, state : {0}", [state]);
                    return false;
                }

                // Checking for server availability
                if (!InternetCheckConnectionA(_CheckUriString, 0x00000001, 0))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    Debug.WriteLine(lastError == ERROR_NOT_CONNECTED ? "No Internet connection" : "Server unavailable");
                    return false;
                }

                return true;
            }
        }
        */
    }
}
