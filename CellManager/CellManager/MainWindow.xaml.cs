using System.Windows;
using CellManager.ViewModels;
using CellManager.Services;

namespace CellManager
{
    /// <summary>
    ///     WPF shell window that hosts the tabbed interface driven by <see cref="MainViewModel"/>.
    ///     The view is largely defined in XAML with minimal code-behind.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        ///     Initializes the component graph declared in XAML.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}