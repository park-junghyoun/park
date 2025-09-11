using System.Windows;
using System.Windows.Controls;
using CellManager.ViewModels;

namespace CellManager.Converters
{
    public class SpecTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate { get; set; } = null!;
        public DataTemplate ComboBoxTemplate { get; set; } = null!;

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
