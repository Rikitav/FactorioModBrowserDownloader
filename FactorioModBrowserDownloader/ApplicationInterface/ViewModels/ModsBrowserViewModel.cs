using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.PresentationFramework;
using FactorioNexus.PresentationFramework.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FactorioNexus.ApplicationInterface.ViewModels
{
    public class ModsBrowserViewModel : ViewModelBase, IModsBrowserViewModel
    {
        private readonly object _lock;
        private readonly ManualResetEventSlim _canQueryNext;
        private readonly CancellCommand _cancellCommand;
        private readonly RefreshCommand _refreshCommand;
        private readonly ObservableCollection<ModEntryFull> _displayModsList;
        private readonly QueryFilterSettings _querySettings;

        private readonly IFactorioNexusClient _nexusClient;
        private readonly IDatabaseIndexer _databaseIndexer;

        private readonly RelayCommand _repopulateCommand;
        private Thread? _refreshThread;

        private bool _requireListExtending = true;
        private bool _isWorking = false;
        private bool _isRepopulating = false;
        private bool _isCriticalError = false;
        private string? _workDescription = null;
        private string? _criticalErrorMessage = null;

        public CancellCommand CancellCommand => _cancellCommand;
        public RefreshCommand RefreshCommand => _refreshCommand;
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

        public ModsBrowserViewModel(IFactorioNexusClient nexusClient, IDatabaseIndexer databaseIndexer)
        {
            _nexusClient = nexusClient;
            _databaseIndexer = databaseIndexer;

            _lock = new object();
            _displayModsList = [];
            _canQueryNext = new ManualResetEventSlim(true);
            _repopulateCommand = new RelayCommand(_ => RepopulateIndexedDatabase());
            _cancellCommand = new CancellCommand();
            _refreshCommand = new RefreshCommand(this);
            _querySettings = new QueryFilterSettings(_refreshCommand);

            CancellCommand.CancellationRequested += (_, _) =>
            {
                IsCriticalError = false;
                CriticalErrorMessage = null;
            };

            ViewInitialized = true;
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

            Debug.WriteLine("Refresh requested");
            try
            {
                IsWorking = true;
                WorkDescription = "Refreshing mods list";
                Dispatcher.FromThread(dispatcherThread).Invoke(() => DisplayModsList.Clear());

                int count = 0;
                foreach (ModEntryInfo entry in _databaseIndexer.GetEntries(QuerySettings, CancellCommand.Token))
                {
                    try
                    {
                        _canQueryNext.Wait(CancellCommand.Token);
                        CancellCommand.Token.ThrowIfCancellationRequested();

                        ModEntryFull fullMod = await _nexusClient.FetchFullModInfo(entry, CancellCommand.Token);
                        if (!QuerySettings.CanPass(fullMod))
                            continue;

                        Dispatcher.FromThread(dispatcherThread).Invoke(() => DisplayModsList.Add(fullMod));
                        count++;
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine("Timed out fetching mod \"{0}\"", [entry.Id]);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("failed to download mod \"{0}\". {1}", [entry.Id, ex]);
                        continue;
                    }
                }

                Debug.WriteLine("{0} entries queried", [count]);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (HttpRequestException hrexc)
            {
                IsCriticalError = true;
                CriticalErrorMessage = string.Format("Failed to make a HTTP request : {0}\nPlease check you internet connection or try disabling VPN", hrexc);
            }
            catch (Exception ex)
            {
                IsCriticalError = true;
                CriticalErrorMessage = ex.ToString();
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
            catch (HttpRequestException hrexc)
            {
                IsCriticalError = true;
                CriticalErrorMessage = string.Format("Failed to make a HTTP request : {0}\nPlease check you internet connection or try disabling VPN", hrexc);
            }
            catch (Exception ex)
            {
                IsCriticalError = true;
                CriticalErrorMessage = ex.ToString();
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
                            Debug.WriteLine("SOMEHOW, cannot resume a page extending.");
                        }

                        break;
                    }
            }
        }
    }
}
