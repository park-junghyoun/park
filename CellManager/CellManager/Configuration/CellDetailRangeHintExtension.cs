using System;
using System.Windows.Markup;

namespace CellManager.Configuration
{
    [MarkupExtensionReturnType(typeof(string))]
    public class CellDetailTextRangeHintExtension : MarkupExtension
    {
        public string? PropertyName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                return string.Empty;
            }

            return CellDetailTextRules.GetRule(PropertyName).CreateRangeDescription();
        }
    }

    [MarkupExtensionReturnType(typeof(string))]
    public class CellDetailNumericRangeHintExtension : MarkupExtension
    {
        public string? PropertyName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                return string.Empty;
            }

            return CellDetailNumericRules.GetRule(PropertyName).CreateRangeDescription();
        }
    }
}
