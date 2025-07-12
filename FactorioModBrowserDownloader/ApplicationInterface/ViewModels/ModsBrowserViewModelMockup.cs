using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.PresentationFramework;
using FactorioNexus.PresentationFramework.Commands;
using System.Collections.ObjectModel;

namespace FactorioNexus.ApplicationInterface.ViewModels
{
    public class ModsBrowserViewModelMockup : ViewModelBase, IModsBrowserViewModel
    {
        public CancellCommand CancellCommand { get; }
        public RefreshCommand RefreshCommand { get; }
        public ObservableCollection<ModEntryFull> DisplayModsList { get; }
        public QueryFilterSettings QuerySettings { get; set; }
        public bool RequireListExtending { get; set; }
        public bool IsWorking { get; set; }
        public bool IsCriticalError { get; set; }
        public string? WorkDescription { get; set; }
        public string? CriticalErrorMessage { get; set; }
        public bool IsRepopulating { get; set; }

        public ModsBrowserViewModelMockup()
        {
            DisplayModsList = [];
            CancellCommand = new CancellCommand();
            RefreshCommand = new RefreshCommand(this);
            QuerySettings = new QueryFilterSettings(RefreshCommand);
            ViewInitialized = true;
        }

        public void RefreshDisplayModsList()
        {
            try
            {
                IsWorking = true;
                WorkDescription = "Refreshing mods list";
                DisplayModsList.Clear();
            }
            catch (OperationCanceledException)
            {
                return;
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

        public void RepopulateIndexedDatabase()
        {

        }
    }
}
