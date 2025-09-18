using System;
using System.Windows.Markup;

namespace CellManager.Configuration
{
    [MarkupExtensionReturnType(typeof(string))]
    public class CellDetailTextRangeHint : MarkupExtension
    {
        public string? PropertyName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                return string.Empty;
            }

            return CellDetailConstraintProvider.TryGetFieldConstraint(PropertyName, out var constraint)
                ? constraint.CreateDescription()
                : string.Empty;
        }
    }

    [MarkupExtensionReturnType(typeof(string))]
    public class CellDetailNumericRangeHint : MarkupExtension
    {
        public string? PropertyName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                return string.Empty;
            }

            return CellDetailConstraintProvider.TryGetFieldConstraint(PropertyName, out var constraint)
                ? constraint.CreateDescription()
                : string.Empty;
        }
    }
}
