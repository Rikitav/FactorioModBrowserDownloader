using System.Windows.Input;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public class CancellCommand(CancellationTokenSource cancellationSource) : ICommand
    {
        private readonly CancellationTokenSource CancellationSource = cancellationSource;
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !CancellationSource.IsCancellationRequested;
        }

        public void Execute(object? parameter)
        {
            CancellationSource.Cancel();
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
