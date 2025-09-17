using System.Windows;

namespace CellManager.Views.CellLibary
{
    /// <summary>Modal dialog that confirms whether a cell should be removed.</summary>
    public partial class DeleteConfirmationView : Window
    {
        public DeleteConfirmationView()
        {
            InitializeComponent();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
