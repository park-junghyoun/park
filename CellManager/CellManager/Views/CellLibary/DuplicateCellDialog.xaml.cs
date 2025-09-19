using System.Windows;

namespace CellManager.Views.CellLibary
{
    /// <summary>Dialog prompting the user for new identifying information when duplicating a cell.</summary>
    public partial class DuplicateCellDialog : Window
    {
        public DuplicateCellDialog()
        {
            InitializeComponent();
        }

        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
