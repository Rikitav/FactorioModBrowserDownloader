using System.Windows.Input;

namespace FactorioNexus.ApplicationInterface.Dependencies
{
    public interface IMainWindowViewModel
    {
        public ICommand OpenDataDirectoryCommand { get; }
    }
}
