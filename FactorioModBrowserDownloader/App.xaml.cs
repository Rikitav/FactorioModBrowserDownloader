using FactorioNexus.Data.Databases;
using FactorioNexus.Infrastructure.Models.Config;
using FactorioNexus.Infrastructure.Services;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.UserInterface.ViewModels;
using FactorioNexus.UserInterface.Views.MainWindow;
using FactorioNexus.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NReco.Logging.File;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FactorioNexus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private static ILogger<App> _logger = NullLogger<App>.Instance;
        private static IServiceProvider _services = default!;
        private static IConfigurationManager _configuration = default!;
        private static ApplicationSetings _settings = null!;

        public static string DataDirectory => Constants.PrivateAppDataDirectory;
        public static ILogger<App> Logger => _logger;
        public static IServiceProvider Services => _services;
        public static IConfigurationManager Configuration => _configuration;
        public static ApplicationSetings Settings => _settings;

        private bool _isDisposed;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Directory.CreateDirectory(DataDirectory);

            // Checking if programm was launched more than once
            if (CheckProcessDuplication())
            {
                Environment.Exit(1);
                return;
            }

            // Creating and configuring app infrastructure
            IServiceCollection servicesCollection = new ServiceCollection();
            IConfigurationManager configurationManager = new ConfigurationManager();
            Configure(servicesCollection, configurationManager);

            // Building providers
            _configuration = configurationManager;
            _services = servicesCollection.BuildServiceProvider();

            // Resolving required services
            _logger = Services.GetRequiredService<ILogger<App>>();
            _settings = Services.GetRequiredService<IOptions<ApplicationSetings>>().Value;
            MainWindow = Services.GetRequiredService<MainWindowMarkup>();

            Settings.ValidateSettingsContainer();
        }

        private static void Configure(IServiceCollection services, IConfigurationManager configuration)
        {
            // Adding configuration files
            configuration
                .AddJsonFile(Path.Combine(Constants.PrivateAppDataDirectory, "config.json"))
                .AddJsonFile("config.json", true);

            // Configuring options
            services
                .Configure<ApplicationSetings>(configuration.GetSection("Application"));

            // Configure Logging
            services.AddLogging(logging => logging
                .AddConsole()
                .AddFile(configuration));

            // Register Services
            services
                .AddSingleton<IDatabaseIndexer, DatabaseIndexer>()
                .AddSingleton<IDependencyResolver, DependencyResolver>()
                .AddSingleton<IDownloadingManager, DownloadingManager>()
                .AddSingleton<IFactorioNexusClient, FactorioNexusClient>()
                .AddSingleton<IStoringManager, StoringManager>()
                .AddSingleton<IThumbnailsResolver, ThumbnailsResolver>();

            // Register ViewModels
            services.AddViewModelLocator(locator => locator
                .AddViewModel<ModsBrowserViewModel, ModsBrowserView>()
                .AddViewModel<ModsStorageViewModel, ModsStorageView>()
                .AddViewModel<ApplicationSettingsViewModel, ApplicationSettingsView>());

            // Registering Database
            services.AddDbContext<IndexedModPortalDatabase>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

            // Register Views
            services.AddSingleton<MainWindowMarkup>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.LogInformation("Application was stopped with exit code {code}", e.ApplicationExitCode);
            Dispose();
            base.OnExit(e);
        }

        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!args.IsTerminating)
                return;

            if (args.ExceptionObject is not Exception exc)
                return;

            Logger.LogCritical(exc, "Application's execution was faulted by unhandled exception in '{sender}'", sender?.ToString());
            MessageBox.Show("Application's execution was faulted by unhandled exception.\nSee logs for more info", AppDomain.CurrentDomain.FriendlyName, MessageBoxButton.OK);
        }

        private static bool IsDesign()
        {
            DependencyObject dummyObject = new DependencyObject();
            return DesignerProperties.GetIsInDesignMode(dummyObject);
        }

        private static bool CheckProcessDuplication()
        {
            Process current = Process.GetCurrentProcess();
            return Process.GetProcessesByName(current.ProcessName).Length > 1;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (Services is IDisposable servicesDisposable)
                servicesDisposable.Dispose();

            if (Configuration is IDisposable configurationDisposable)
                configurationDisposable.Dispose();

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }
    }
}
