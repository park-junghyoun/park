using System.Windows.Controls;
using CellManager.Models;

namespace CellManager.Views.Controls
{
    public partial class TimelinePreviewControl : UserControl
    {
        public TimelinePreviewControl()
        {
            InitializeComponent();
        }

        public Schedule? Schedule { get; set; }
    }
}

