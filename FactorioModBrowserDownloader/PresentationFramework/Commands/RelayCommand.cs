using System.Windows.Input;

namespace FactorioNexus.PresentationFramework.Commands
{
    public class RelayCommand(Action<object?>? execute, Func<object?, bool>? canExecute) : ICommand
    {
        private readonly Action<object?>? _execute = execute;
        private readonly Func<object?, bool>? _canExecute = canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object?>? execute)
            : this(execute, null) { }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}
