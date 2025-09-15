using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace FactorioNexus.UserInterface.ViewModels
{
    public interface IViewModel<F> where F : FrameworkElement;
    public abstract class ViewModelBase<F> : ObservableObject, IViewModel<FrameworkElement> where F : FrameworkElement;
}
