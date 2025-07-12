using FactorioNexus.ApplicationInterface.Dependencies;
using System.Windows.Input;

namespace FactorioNexus.PresentationFramework.Commands
{
    public class RefreshCommand(IModsBrowserViewModel viewModel) : ICommand
    {
        private readonly IModsBrowserViewModel _viewModel = viewModel;
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _viewModel.RefreshDisplayModsList();
        }
    }
}
