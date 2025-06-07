using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace FactorioModBrowserDownloader.Controls.Converters
{
    [TypeConverter(typeof(DateTime))]
    public class RelativeDateTimeConverter : MarkupExtension, IValueConverter
    {
        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dateTime)
                throw new ArgumentException("Converting value cannot be not DateTime", nameof(value));

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * Minute)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * Minute)
                return "a minute ago";

            if (delta < 45 * Minute)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * Minute)
                return "an hour ago";

            if (delta < 24 * Hour)
                return ts.Hours + " hours ago";

            if (delta < 48 * Hour)
                return "yesterday";

            if (delta < 30 * Day)
                return ts.Days + " days ago";

            if (delta < 12 * Month)
            {
                int months = System.Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = System.Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
