using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace FactorioNexus.UserInterface.Extensions.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool state)
                throw new ArgumentException("parameter must be string type", nameof(value));

            if (Inverse)
                state = !state;

            return state ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
