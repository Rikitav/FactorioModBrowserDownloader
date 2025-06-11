using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace FactorioNexus.ApplicationPresentation.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class TextToHintVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string str)
                throw new ArgumentException();

            return string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
