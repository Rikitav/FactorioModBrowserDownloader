using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.Infrastructure.Models;
using FactorioNexus.UserInterface.Extensions.Commands;
using FactorioNexus.UserInterface.Views.MainWindow;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FactorioNexus.UserInterface.ViewModels.Abstractions
{
    public interface IModsBrowserViewModel : IViewModel<ModsBrowserView>
    {
        public CancellCommand CancellCommand { get; }
        public ICommand RefreshCommand { get; }
        public ObservableCollection<ModEntryFull> DisplayModsList { get; }
        public QueryFilterSettings QuerySettings { get; }
        public bool RequireListExtending { get; set; }
        public bool IsWorking { get; set; }
        public bool IsRepopulating { get; }
        public bool IsCriticalError { get; }
        public string? WorkDescription { get; }
        public string? CriticalErrorMessage { get; }

        public void RefreshDisplayModsList();
        public void RepopulateIndexedDatabase();
    }
}
