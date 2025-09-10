using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CellManager.Models;
using CellManager.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CellManager.Views.Controls
{
    public partial class TimelinePreviewControl : UserControl
    {
        public static readonly DependencyProperty SelectedScheduleProperty =
            DependencyProperty.Register(nameof(SelectedSchedule), typeof(Schedule), typeof(TimelinePreviewControl),
                new PropertyMetadata(null, OnDataChanged));

        public static readonly DependencyProperty StepsProperty =
            DependencyProperty.Register(nameof(Steps), typeof(ObservableCollection<StepTemplate>), typeof(TimelinePreviewControl),
                new PropertyMetadata(null, OnStepsChanged));

        public Schedule? SelectedSchedule
        {
            get => (Schedule?)GetValue(SelectedScheduleProperty);
            set => SetValue(SelectedScheduleProperty, value);
        }

        public ObservableCollection<StepTemplate>? Steps
        {
            get => (ObservableCollection<StepTemplate>?)GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        public PlotModel PlotModel { get; } = new PlotModel { Title = "Schedule" };

        public TimelinePreviewControl()
        {
            InitializeComponent();
            ConfigurePlot();
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimelinePreviewControl)d;
            control.UpdatePlot();
        }

        private static void OnStepsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimelinePreviewControl)d;
            if (e.OldValue is ObservableCollection<StepTemplate> old)
                old.CollectionChanged -= control.OnCollectionChanged;
            if (e.NewValue is ObservableCollection<StepTemplate> @new)
                @new.CollectionChanged += control.OnCollectionChanged;
            control.UpdatePlot();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdatePlot();

        private void ConfigurePlot()
        {
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time (s)",
                IsPanEnabled = true,
                IsZoomEnabled = true
            });
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Value",
                IsPanEnabled = true,
                IsZoomEnabled = true
            });
        }

        private void UpdatePlot()
        {
            PlotModel.Series.Clear();
            if (Steps == null || Steps.Count == 0)
            {
                PlotModel.InvalidatePlot(true);
                return;
            }

            var voltageSeries = new LineSeries { Title = "Voltage", TrackerFormatString = "{0}\nTime: {1:0.##} s\nVoltage: {2:0.##} V" };
            var currentSeries = new LineSeries { Title = "Current", TrackerFormatString = "{0}\nTime: {1:0.##} s\nCurrent: {2:0.##} A" };

            double time = 0;
            foreach (var step in Steps)
            {
                var duration = step.Duration.TotalSeconds;
                var (voltage, current) = ParseParameters(step.Parameters);

                if (voltage.HasValue)
                {
                    voltageSeries.Points.Add(new DataPoint(time, voltage.Value));
                    voltageSeries.Points.Add(new DataPoint(time + duration, voltage.Value));
                }

                if (current.HasValue)
                {
                    currentSeries.Points.Add(new DataPoint(time, current.Value));
                    currentSeries.Points.Add(new DataPoint(time + duration, current.Value));
                }

                time += duration;
            }

            if (voltageSeries.Points.Count > 0)
                PlotModel.Series.Add(voltageSeries);
            if (currentSeries.Points.Count > 0)
                PlotModel.Series.Add(currentSeries);

            PlotModel.InvalidatePlot(true);
        }

        private static (double? voltage, double? current) ParseParameters(string? parameters)
        {
            double? voltage = null;
            double? current = null;
            if (!string.IsNullOrEmpty(parameters))
            {
                var voltMatch = Regex.Match(parameters, @"([\d\.]+)\s*[Vv]");
                if (voltMatch.Success && double.TryParse(voltMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    voltage = v;
                var currMatch = Regex.Match(parameters, @"([\d\.]+)\s*[Aa]");
                if (currMatch.Success && double.TryParse(currMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var c))
                    current = c;
            }
            return (voltage, current);
        }
    }
}
