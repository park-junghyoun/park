using System.Windows;

namespace CellManager.Views.CellLibary
{
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
