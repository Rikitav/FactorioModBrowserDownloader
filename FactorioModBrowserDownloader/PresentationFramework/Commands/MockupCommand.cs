using System.Windows.Input;

#pragma warning disable CS0067
namespace FactorioNexus.PresentationFramework.Commands
{
    public sealed class MockupCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _ = 0xBAD + 0xC0DE;
        }
    }
}
