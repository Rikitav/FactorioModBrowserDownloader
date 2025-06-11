using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            field = value;
            NotifyPropertyChanged(propertyName);
            OnPropertyChanged(propertyName);
        }

        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {

        }
    }
}
