using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CellManager.Converters
{
    public class PercentageToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (double.IsNaN(percentage) || double.IsInfinity(percentage))
                    percentage = 0;

                percentage = Math.Max(0, percentage);
                return new GridLength(percentage, GridUnitType.Star);
            }

            return new GridLength(0, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
