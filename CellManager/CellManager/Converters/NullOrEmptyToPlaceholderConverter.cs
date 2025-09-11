using System;
using System.Globalization;
using System.Windows.Data;

namespace CellManager.Converters
{
    public class NullOrEmptyToPlaceholderConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var placeholder = parameter as string ?? string.Empty;
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            return placeholder;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

