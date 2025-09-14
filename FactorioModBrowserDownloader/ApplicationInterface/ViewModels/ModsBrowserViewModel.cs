using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationArchitecture.Services;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.PresentationFramework;
using FactorioNexus.PresentationFramework.Commands;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FactorioNexus.ApplicationInterface.ViewModels
{
    public class ModsBrowserViewModel : ViewModelBase, IModsBrowserViewModel
    {
        private readonly object _lock;
        private readonly ManualResetEventSlim _canQueryNext;
        private readonly CancellCommand _cancellCommand;
        private readonly RelayCommand _refreshCommand;
        private readonly ObservableCollection<ModEntryFull> _displayModsList;
        private readonly QueryFilterSettings _querySettings;

        private readonly IFactorioNexusClient _nexusClient;
        private readonly IDatabaseIndexer _databaseIndexer;
        private readonly ILogger<ModsBrowserViewModel> _logger;

        private readonly RelayCommand _repopulateCommand;
        private Thread? _refreshThread;

        private bool _requireListExtending = true;
        private bool _isWorking = false;
        private bool _isRepopulating = false;
        private bool _isCriticalError = false;
        private string? _workDescription = null;
        private string? _criticalErrorMessage = null;

        public ICommand CancellCommand => _cancellCommand;
        public ICommand RefreshCommand => _refreshCommand;
        public ObservableCollection<ModEntryFull> DisplayModsList => _displayModsList;
        public QueryFilterSettings QuerySettings => _querySettings;

        public RelayCommand RepopulateCommand => _repopulateCommand;

        public bool RequireListExtending
        {
            get => _requireListExtending;
            set => Set(ref _requireListExtending, value);
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }

        public bool IsRepopulating
        {
            get => _isRepopulating;
            set => Set(ref _isRepopulating, value);
        }

        public bool IsCriticalError
        {
            get => _isCriticalError;
            set => Set(ref _isCriticalError, value);
        }

        public string? WorkDescription
        {
            get => _workDescription;
            set => Set(ref _workDescription, value);
        }

        public string? CriticalErrorMessage
        {
            get => _criticalErrorMessage;
            set => Set(ref _criticalErrorMessage, value);
        }

        public ModsBrowserViewModel(IFactorioNexusClient nexusClient, IDatabaseIndexer databaseIndexer, ILogger<ModsBrowserViewModel> logger)
        {
            _nexusClient = nexusClient;
            _databaseIndexer = databaseIndexer;
            _logger = logger;

            _lock = new object();
            _displayModsList = [];
            _canQueryNext = new ManualResetEventSlim(true);
            _cancellCommand = new CancellCommand();
            _repopulateCommand = new RelayCommand(_ => RepopulateIndexedDatabase());
            _refreshCommand = new RelayCommand(_ => RefreshDisplayModsList());
            _querySettings = new QueryFilterSettings(_refreshCommand);

            _cancellCommand.CancellationRequested += (_, _) =>
            {
                IsCriticalError = false;
                CriticalErrorMessage = null;
            };

            ViewInitialized = true;
            _logger.LogInformation("ModsBrowserViewModel initialized");

            _refreshCommand.Execute(null);
        }

        public void RefreshDisplayModsList()
        {
            if (!ViewInitialized)
                return;

            CancellCommand.Execute(null);
            _refreshThread = new Thread(RefreshDisplayModsListInner);
            _refreshThread.Start(Thread.CurrentThread);
        }

        private async void RefreshDisplayModsListInner(object? parameter)
        {
            if (!ViewInitialized)
                return;

            if (parameter is null || parameter is not Thread dispatcherThread)
                throw new Exception();

            _logger.LogInformation("Browser efresh requested");
            try
            {
                IsWorking = true;
                WorkDescription = "Refreshing mods list";
                Dispatcher.FromThread(dispatcherThread).Invoke(() => DisplayModsList.Clear());

                IEnumerable<ModEntryInfo> queriedEntries = _databaseIndexer.GetEntries(QuerySettings, _cancellCommand.Token);
                int count = queriedEntries.Count();
                _logger.LogInformation("Queried {count} entries from database", count);

                count = 0;
                foreach (ModEntryInfo entry in queriedEntries)
                {
                    try
                    {
                        _canQueryNext.Wait(_cancellCommand.Token);
                        _cancellCommand.Token.ThrowIfCancellationRequested();

                        ModEntryFull fullMod = await _nexusClient.FetchFullModInfo(entry, _cancellCommand.Token);
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
                CriticalErrorMessage =  string.Format("Browser refreshing sequence was interupted by unhandled exception. {0}", ex.Message.ToString());
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
                using JsonDocument document = await _nexusClient.GetModsDatabase(_cancellCommand.Token);

                WorkDescription = "Repopulating database";
                await _databaseIndexer.RepopulateFrom(document, _cancellCommand.Token);
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

        public override void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(RequireListExtending):
                    {
                        try
                        {
                            if (!ViewInitialized)
                                return;

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
