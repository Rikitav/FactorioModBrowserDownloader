using System.Windows.Input;

namespace FactorioNexus.UserInterface.ViewModels.Abstractions
{
    public interface IMainWindowViewModel
    {
        public ICommand OpenDataDirectoryCommand { get; }
    }
}
