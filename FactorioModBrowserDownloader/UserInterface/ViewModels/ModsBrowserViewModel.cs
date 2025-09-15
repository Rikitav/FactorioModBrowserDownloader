using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.Infrastructure.Models;
using FactorioNexus.Infrastructure.Services;
using FactorioNexus.Infrastructure.Services.Abstractions;
using FactorioNexus.UserInterface.Extensions.Commands;
using FactorioNexus.UserInterface.ViewModels.Abstractions;
using FactorioNexus.UserInterface.Views.MainWindow;
using FactorioNexus.Utilities;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FactorioNexus.UserInterface.ViewModels
{
    public partial class ModsBrowserViewModel : ViewModelBase<ModsBrowserView>, IModsBrowserViewModel
    {
        private readonly object _lock = new object();
        private readonly ManualResetEventSlim _canQueryNext = new ManualResetEventSlim(true);

        private readonly IFactorioNexusClient _nexusClient;
        private readonly IDatabaseIndexer _databaseIndexer;
        private readonly ILogger<ModsBrowserViewModel> _logger;

        private Thread? _refreshThread;

        [ObservableProperty]
        private bool _requireListExtending = true;

        [ObservableProperty]
        private bool _isWorking = false;

        [ObservableProperty]
        private bool _isRepopulating = false;

        [ObservableProperty]
        private bool _isCriticalError = false;

        [ObservableProperty]
        private string? _workDescription = null;

        [ObservableProperty]
        private string? _criticalErrorMessage = null;

        public ObservableCollection<ModEntryFull> DisplayModsList { get; } = [];
        public CancellCommand CancellCommand { get; } = new CancellCommand();
        public QueryFilterSettings QuerySettings { get; }
        public ICommand RefreshCommand { get; }
        public ICommand RepopulateCommand { get; }

        public ModsBrowserViewModel(IFactorioNexusClient nexusClient, IDatabaseIndexer databaseIndexer, ILogger<ModsBrowserViewModel> logger)
        {
            _nexusClient = nexusClient;
            _databaseIndexer = databaseIndexer;
            _logger = logger;

            RepopulateCommand = new RelayCommand(RepopulateIndexedDatabase);
            RefreshCommand = new RelayCommand(RefreshDisplayModsList);
            QuerySettings = new QueryFilterSettings(RefreshCommand);

            CancellCommand.CancellationRequested += (_, _) =>
            {
                IsCriticalError = false;
                CriticalErrorMessage = null;
            };

            _logger.LogInformation("ModsBrowserViewModel initialized");
            RefreshCommand.Execute(null);
        }

        public void RefreshDisplayModsList()
        {
            CancellCommand.Execute(null);
            _refreshThread = new Thread(RefreshDisplayModsListInner);
            _refreshThread.Start(Thread.CurrentThread);
        }

        private async void RefreshDisplayModsListInner(object? parameter)
        {
            if (parameter is not Thread { } dispatcherThread)
                throw new ArgumentException("thread start method parameter should be Thread obj", nameof(parameter));

            _logger.LogInformation("Browser efresh requested");
            try
            {
                IsWorking = true;
                WorkDescription = "Refreshing mods list";
                Dispatcher.FromThread(dispatcherThread).Invoke(() => DisplayModsList.Clear());

                IEnumerable<ModEntryInfo> queriedEntries = _databaseIndexer.GetEntries(QuerySettings, CancellCommand.Token);
                int count = queriedEntries.Count();
                _logger.LogInformation("Queried {count} entries from database", count);

                count = 0;
                foreach (ModEntryInfo entry in queriedEntries)
                {
                    try
                    {
                        _canQueryNext.Wait(CancellCommand.Token);
                        CancellCommand.Token.ThrowIfCancellationRequested();

                        ModEntryFull fullMod = await _nexusClient.FetchFullModInfo(entry, CancellCommand.Token);
                        if (!QuerySettings.CanPass(fullMod))
                            continue;

                        Dispatcher.FromThread(dispatcherThread).Invoke(() => DisplayModsList.Add(fullMod));
                        _logger.LogTrace("Added '{id}' to display list", fullMod.Id);
                        count++;
                    }
                    catch (RequestException rexc) when (rexc.Aggreagate<TimeoutException>())
                    {
                        _logger.LogError("Timed out fetching mod \"{modID}\"", entry.Id);
                        continue;
                    }
                    catch (RequestException rexc)
                    {
                        IsCriticalError = true;
                        CriticalErrorMessage = string.Format("Failed to make a HTTP request : {0}\nPlease check you internet connection or try disabling VPN", rexc.InnerException?.Message);
                        _logger.LogError("Failed to make a HTTP request. {exception}", rexc);
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        _ = 0xBAD + 0xC0DE;
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("failed to download mod \"{modID}\". {exception}", entry.Id, ex.Message);
                        continue;
                    }
                }

                if (count == 0)
                {
                    IsCriticalError = true;
                    CriticalErrorMessage = "No mods was found by this filter options";
                    _logger.LogError("No mods was found by this filter options");
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                _ = 0xBAD + 0xC0DE;
                return;
            }
            catch (Exception ex)
            {
                IsCriticalError = true;
                CriticalErrorMessage = string.Format("Browser refreshing sequence was interupted by unhandled exception. {0}", ex.Message.ToString());
                _logger.LogError("Browser refreshing sequence was interupted by unhandled exception. {exception}", ex);
                return;
            }
            finally
            {
                IsWorking = false;
                WorkDescription = null;
            }
        }

        public async void RepopulateIndexedDatabase()
        {
            try
            {
                IsWorking = true;
                IsRepopulating = true;
                CancellCommand.Execute(null);
                DisplayModsList.Clear();

                WorkDescription = "Requesting database";
                using JsonDocument document = await _nexusClient.GetModsDatabase(CancellCommand.Token);

                WorkDescription = "Repopulating database";
                await _databaseIndexer.RepopulateFrom(document, CancellCommand.Token);
                MessageBox.Show("Local database re-population completed!");
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (RequestException rexc)
            {
                IsCriticalError = true;
                CriticalErrorMessage = string.Format("Failed to make a HTTP request : {0}\nPlease check you internet connection or try disabling VPN", rexc);
                return;
            }
            catch (HttpRequestException hrexc)
            {
                IsCriticalError = true;
                CriticalErrorMessage = string.Format("Failed to make a HTTP request : {0}\nPlease check you internet connection or try disabling VPN", hrexc);
                return;
            }
            catch (Exception ex)
            {
                IsCriticalError = true;
                CriticalErrorMessage = ex.ToString();
                return;
            }
            finally
            {
                IsWorking = false;
                IsRepopulating = false;
                WorkDescription = null;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RequireListExtending):
                    {
                        try
                        {
                            if (RequireListExtending)
                                _canQueryNext.Set();
                            else
                                _canQueryNext.Reset();
                        }
                        catch
                        {
                            _logger.LogError("Failed to resume a page extending.");
                        }

                        break;
                    }
            }
        }
    }
}
