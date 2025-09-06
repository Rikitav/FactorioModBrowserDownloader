using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace FactorioNexus.PresentationFramework.Extensions
{
    public class ViewModelExtension : MarkupExtension
    {
        private static readonly DependencyObject _dummyObject = new DependencyObject();

        public Type? Type { get; set; }
        public Type? Mockup { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                Type? toResolve = Mockup != null && DesignerProperties.GetIsInDesignMode(_dummyObject) ? Mockup : Type;
                if (toResolve == null)
                    return new object();

                return App.Services.GetRequiredService(toResolve);
            }
            catch
            {
                return new object();
            }
        }
    }
}
