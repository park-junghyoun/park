using System;
using System.Globalization;
using System.Windows.Data;

namespace CellManager.Converters
{
    /// <summary>
    ///     Replaces null or whitespace strings with a XAML-supplied placeholder so that empty fields
    ///     still render helpful text in the UI.
    /// </summary>
    public class NullOrEmptyToPlaceholderConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var placeholder = parameter as string ?? string.Empty;
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            return placeholder;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

