using System;
using System.Globalization;
using System.Windows.Data;

namespace CellManager.Views.Converters
{
    public class DurationToWidthConverter : IValueConverter
    {
        public double Factor { get; set; } = 10;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan span)
                return span.TotalMinutes * Factor;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
