using System.Windows;

namespace CellManager.Views.TestSetup
{
    public partial class ProfileDetailWindow : Window
    {
        public ProfileDetailWindow()
        {
            InitializeComponent();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
