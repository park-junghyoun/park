using System.Windows;
using System.Windows.Controls;
using CellManager.ViewModels;

namespace CellManager.Converters
{
    /// <summary>
    ///     Chooses between textbox and combobox templates when editing protection profile settings.
    /// </summary>
    public class SpecTemplateSelector : DataTemplateSelector
    {
        /// <summary>Template for free-form text input values.</summary>
        public DataTemplate TextBoxTemplate { get; set; } = null!;

        /// <summary>Template for constrained options presented as a combo box.</summary>
        public DataTemplate ComboBoxTemplate { get; set; } = null!;

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ProtectionSetting setting && setting.Options.Count > 0)
            {
                return ComboBoxTemplate;
            }
            return TextBoxTemplate;
        }
    }
}
