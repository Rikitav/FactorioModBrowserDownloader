using System.Windows.Input;

#pragma warning disable CS0067
namespace FactorioNexus.PresentationFramework.Commands
{
    public class CancellCommand : ICommand
    {
        private readonly object _lock = new object();
        private CancellationTokenSource CancellationSource = new CancellationTokenSource();
        public CancellationToken Token => CancellationSource.Token;

        public event EventHandler? CanExecuteChanged;
        public event EventHandler? CancellationRequested;

        public bool CanExecute(object? parameter)
        {
            return !CancellationSource.IsCancellationRequested;
        }

        public void Execute(object? parameter)
        {
            lock (_lock)
            {
                CancellationRequested?.Invoke(this, new EventArgs());
                //CanExecuteChanged?.Invoke(this, new EventArgs());

                //IsCriticalError = false;
                //CriticalErrorMessage = null;

                CancellationSource.Cancel();
                CancellationSource.Dispose();
                CancellationSource = new CancellationTokenSource();
            }
        }
    }
}
