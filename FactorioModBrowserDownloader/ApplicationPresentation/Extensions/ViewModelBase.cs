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
                throw new ArgumentNullException(nameof(propertyName));

            field = value;
            RaisePropertyChanged(propertyName);
            OnPropertyChanged(propertyName);
        }

        protected void Set(Action action, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            action.Invoke();
            RaisePropertyChanged(propertyName);
            OnPropertyChanged(propertyName);
        }

        protected T Get<T>(ref T? field, Func<T> defaultValue, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (field == null)
            {
                field = defaultValue.Invoke();
                RaisePropertyChanged(propertyName);
                OnPropertyChanged(propertyName);
            }

            return field;
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
