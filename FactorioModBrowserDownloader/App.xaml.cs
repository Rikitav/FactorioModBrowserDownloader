using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Services;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.ApplicationInterface.ViewModels;
using FactorioNexus.ApplicationPresentation.Markups.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services.AddLogging(logging => logging.AddConsole());

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
    }
}
