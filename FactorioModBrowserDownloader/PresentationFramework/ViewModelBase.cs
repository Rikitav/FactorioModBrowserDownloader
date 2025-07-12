using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FactorioNexus.PresentationFramework
{
    public abstract class ViewModelBase : IViewModel
    {
        private bool _viewInitialized = false;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
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
        }

        protected void Set(Action action, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            action.Invoke();
            RaisePropertyChanged(propertyName);
        }

        protected T Get<T>(ref T? field, Func<T> defaultValue, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (field == null)
            {
                field = defaultValue.Invoke();
                RaisePropertyChanged(propertyName);
            }

            return field;
        }

        protected T GetAsync<T>(ref T? field, Func<Task<T>> defaultValue, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (field == null)
            {
                field = defaultValue.Invoke().Result;
                RaisePropertyChanged(propertyName);
            }

            return field;
        }

        public void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        public virtual void OnPropertyChanged(string propertyName)
        {

        }
    }
}
