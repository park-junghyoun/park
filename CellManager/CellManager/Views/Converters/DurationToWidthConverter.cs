using System;
using System.Globalization;
using System.Windows.Data;

namespace CellManager.Views.Converters
{
    public class DurationToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 &&
                values[0] is TimeSpan duration &&
                values[1] is TimeSpan total &&
                values[2] is double totalWidth &&
                total.TotalSeconds > 0)
            {
                return totalWidth * (duration.TotalSeconds / total.TotalSeconds);
            }

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
