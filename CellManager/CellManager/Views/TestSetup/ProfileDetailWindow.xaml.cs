using System.Windows;

namespace CellManager.Views.TestSetup
{
    /// <summary>Dialog wrapper used to edit the various profile types.</summary>
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
