using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CellManager.ViewModels;

namespace CellManager.Views.Controls
{
    /// <summary>Reusable control that visualises schedule steps without editing affordances.</summary>
    public partial class TimelinePreviewControl : UserControl
    {
        public TimelinePreviewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the timeline steps rendered by the preview.
        /// </summary>
        public IEnumerable<StepTemplate>? Steps
        {
            get => (IEnumerable<StepTemplate>?)GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        public static readonly DependencyProperty StepsProperty =
            DependencyProperty.Register(
                nameof(Steps),
                typeof(IEnumerable<StepTemplate>),
                typeof(TimelinePreviewControl),
                new PropertyMetadata(null));
    }
}

