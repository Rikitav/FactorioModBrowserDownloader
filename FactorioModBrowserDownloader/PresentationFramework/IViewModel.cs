using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FactorioNexus.PresentationFramework
{
    public interface IViewModel : INotifyPropertyChanged
    {
        void RaisePropertyChanged([CallerMemberName] string? propertyName = null);
        void OnPropertyChanged(string propertyName);
    }
}
