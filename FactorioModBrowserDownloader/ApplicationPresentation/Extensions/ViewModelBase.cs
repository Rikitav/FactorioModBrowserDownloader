using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private bool _viewInitialized = false;

        public bool ViewInitialized
        {
            get => _viewInitialized;
            set => Set(ref _viewInitialized, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            field = value;
            RaisePropertyChanged(propertyName);
            OnPropertyChanged(propertyName);
        }

        protected void Set(Action action, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            action.Invoke();
            RaisePropertyChanged(propertyName);
            OnPropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {

        }
    }
}
