using System.Windows;
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

        /// <summary>
        /// Gets or sets the <see cref="Schedule"/> displayed by this control.
        /// </summary>
        public Schedule? Schedule
        {
            get => (Schedule?)GetValue(ScheduleProperty);
            set => SetValue(ScheduleProperty, value);
        }

        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register(
                nameof(Schedule),
                typeof(Schedule),
                typeof(TimelinePreviewControl),
                new PropertyMetadata(default(Schedule)));
    }
}

