using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.PresentationFramework;
using FactorioNexus.PresentationFramework.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FactorioNexus.ApplicationInterface.ViewModels
{
    public class ModsBrowserViewModelMockup : ViewModelBase, IModsBrowserViewModel
    {
        public ICommand CancellCommand { get; }
        public ICommand RefreshCommand { get; }
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
            CancellCommand = new MockupCommand();
            RefreshCommand = new MockupCommand();
            QuerySettings = new QueryFilterSettings(RefreshCommand);
            ViewInitialized = true;
        }

        public void RefreshDisplayModsList()
        {

        }

        public void RepopulateIndexedDatabase()
        {

        }
    }
}
