using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.Infrastructure.Models;
using FactorioNexus.UserInterface.Extensions.Commands;
using FactorioNexus.UserInterface.ViewModels.Abstractions;
using FactorioNexus.UserInterface.Views.MainWindow;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FactorioNexus.UserInterface.ViewModels.Mockups
{
    public class ModsBrowserViewModelMockup : ViewModelBase<ModsBrowserView>, IModsBrowserViewModel
    {
        public CancellCommand CancellCommand { get; }
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
            CancellCommand = new CancellCommand();
            RefreshCommand = new MockupCommand();
            QuerySettings = new QueryFilterSettings(RefreshCommand);
        }

        public void RefreshDisplayModsList()
        {

        }

        public void RepopulateIndexedDatabase()
        {

        }
    }
}
