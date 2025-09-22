using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace CellManager.Converters
{
    public class LastItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable)
            {
                object lastItem = null;

                foreach (var item in enumerable)
                {
                    lastItem = item;
                }

                if (lastItem != null)
                {
                    return lastItem;
                }
            }

            return parameter ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
