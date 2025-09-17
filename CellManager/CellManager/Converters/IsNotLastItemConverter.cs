using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CellManager.Converters
{
    /// <summary>
    ///     Converts an item's index and collection size into a <see cref="Visibility"/> value
    ///     so list separators can be hidden for the final item.
    /// </summary>
    public class IsNotLastItemConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Visibility.Visible;
            if (values[0] is int index && values[1] is int count)
            {
                return index < count - 1 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
