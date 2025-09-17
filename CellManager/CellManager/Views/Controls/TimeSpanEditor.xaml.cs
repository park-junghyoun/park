using System.Windows;
using System.Windows.Controls;

namespace CellManager.Views.Controls
{
    /// <summary>Simple composite control for editing hours, minutes, and seconds.</summary>
    public partial class TimeSpanEditor : UserControl
    {
        public TimeSpanEditor()
        {
            InitializeComponent();
        }

        public int Hours
        {
            get => (int)GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }

        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register(
                nameof(Hours),
                typeof(int),
                typeof(TimeSpanEditor),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int Minutes
        {
            get => (int)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register(
                nameof(Minutes),
                typeof(int),
                typeof(TimeSpanEditor),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int Seconds
        {
            get => (int)GetValue(SecondsProperty);
            set => SetValue(SecondsProperty, value);
        }

        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register(
                nameof(Seconds),
                typeof(int),
                typeof(TimeSpanEditor),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
