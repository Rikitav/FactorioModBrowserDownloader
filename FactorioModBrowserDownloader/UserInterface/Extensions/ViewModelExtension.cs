using FactorioNexus.UserInterface.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Markup;

namespace FactorioNexus.UserInterface.Extensions
{
    public class ViewModelExtension : MarkupExtension
    {
        private static readonly DependencyObject _dummyObject = new DependencyObject();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (App.Services == null)
                return new object();

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            try
            {
                IProvideValueTarget? target = serviceProvider.GetService<IProvideValueTarget>();
                if (target == null)
                    return new object();

                Type toResolve = typeof(IViewModel<>).MakeGenericType(target.TargetObject.GetType());
                return App.Services.GetRequiredService(toResolve);
            }
            catch
            {
                return new object();
            }
        }
    }
}
