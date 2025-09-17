using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Provides a rich, design-time friendly façade for the analysis tab including mock data and commands.
    /// </summary>
    public partial class AnalysisViewModel : ObservableObject
    {
        public string HeaderText { get; } = "Analysis";
        public string IconName { get; } = "ChartAreaspline";

        public ObservableCollection<AnalysisDataset> Datasets { get; } = new();
        public ObservableCollection<AnalysisTemplate> SessionTemplates { get; } = new();
        public ObservableCollection<AnalysisWarning> Warnings { get; } = new();
        public ObservableCollection<ComparisonSeriesOption> ChartSeriesOptions { get; } = new();
        public ObservableCollection<ChartModeOption> ChartModes { get; } = new();
        public ObservableCollection<AnalysisOperationStatus> AutomaticAnalyses { get; } = new();
        public ObservableCollection<AnalysisResultSummary> PinnedResults { get; } = new();
        public ObservableCollection<SessionSnapshot> LoadedSessionHistory { get; } = new();
        public ObservableCollection<string> NormalizationOptions { get; } = new();
        public ObservableCollection<string> ResampleOptions { get; } = new();
        public ObservableCollection<string> AnalysisLog { get; } = new();

        [ObservableProperty]
        private bool _isViewEnabled = true;

        [ObservableProperty]
        private AnalysisDataset? _selectedDataset;

        [ObservableProperty]
        private SessionSnapshot? _selectedSessionSnapshot;

        [ObservableProperty]
        private bool _isCompareMode;

        [ObservableProperty]
        private bool _isPinnedResultsExpanded = true;

        [ObservableProperty]
        private double _alignmentOffset;

        [ObservableProperty]
        private string? _selectedNormalization;

        [ObservableProperty]
        private string? _selectedResampleOption;

        [ObservableProperty]
        private ChartModeOption? _selectedChartMode;

        [ObservableProperty]
        private string _currentCellLabel = "No cell selected";

        [ObservableProperty]
        private string _totalTestDurationText = "0h 00m";

        [ObservableProperty]
        private string _voltageSummary = "- / - / -";

        [ObservableProperty]
        private string _currentSummary = "- / - / -";

        [ObservableProperty]
        private string _energySummary = "-";

        [ObservableProperty]
        private string _selectionSummary = "No range selected";

        [ObservableProperty]
        private string _selectionVoltageSummary = string.Empty;

        [ObservableProperty]
        private string _selectionDurationText = string.Empty;

        [ObservableProperty]
        private string _sampleRateStatus = "Not analysed";

        [ObservableProperty]
        private string _lastAnalysisTimestampText = "Never";

        [ObservableProperty]
        private string _baselineAlignmentHint = "0.0 s";

        [ObservableProperty]
        private int _loadedFileCount;

        [ObservableProperty]
        private bool _hasWarnings;


        public IRelayCommand LoadRawCommand { get; }
        public IRelayCommand ReloadCommand { get; }
        public IRelayCommand SaveSessionCommand { get; }
        public IRelayCommand RemoveSelectedDatasetCommand { get; }
        public IRelayCommand<AnalysisDataset> RemoveDatasetCommand { get; }
        public IRelayCommand SaveTemplateCommand { get; }
        public IRelayCommand<AnalysisTemplate> ApplyTemplateCommand { get; }
        public IRelayCommand<AnalysisTemplate> DeleteTemplateCommand { get; }
        public IRelayCommand SaveComparisonPresetCommand { get; }
        public IRelayCommand LoadComparisonPresetCommand { get; }
        public IRelayCommand ExportReportCommand { get; }

        public AnalysisViewModel()
        {
            LoadRawCommand = new RelayCommand(AddMockDataset);
            ReloadCommand = new RelayCommand(() => AnalysisLog.Add("Reload requested"));
            SaveSessionCommand = new RelayCommand(() => AnalysisLog.Add("Session saved"));
            RemoveSelectedDatasetCommand = new RelayCommand(() =>
            {
                if (SelectedDataset != null)
                {
                    RemoveDataset(SelectedDataset);
                }
            });
            RemoveDatasetCommand = new RelayCommand<AnalysisDataset>(dataset =>
            {
                if (dataset != null)
                {
                    RemoveDataset(dataset);
                }
            });
            SaveTemplateCommand = new RelayCommand(SaveCurrentTemplate);
            ApplyTemplateCommand = new RelayCommand<AnalysisTemplate>(template =>
            {
                if (template == null)
                {
                    return;
                }

                AnalysisLog.Add($"Template '{template.Name}' applied");
                SelectedChartMode = ChartModes.FirstOrDefault(mode => mode.Name == template.PreferredChartMode);
                SelectedNormalization = NormalizationOptions.FirstOrDefault(o => o == template.NormalizationOption);
                SelectedResampleOption = ResampleOptions.FirstOrDefault(o => o == template.ResampleOption);
            });
            DeleteTemplateCommand = new RelayCommand<AnalysisTemplate>(template =>
            {
                if (template == null)
                {
                    return;
                }

                SessionTemplates.Remove(template);
                AnalysisLog.Add($"Template '{template.Name}' removed");
            });
            SaveComparisonPresetCommand = new RelayCommand(() => AnalysisLog.Add("Comparison preset saved"));
            LoadComparisonPresetCommand = new RelayCommand(() => AnalysisLog.Add("Comparison preset loaded"));
            ExportReportCommand = new RelayCommand(() =>
            {
                AnalysisLog.Add("Report export queued");
                var now = DateTime.Now;
                LastAnalysisTimestampText = now.ToString("yyyy-MM-dd HH:mm");
            });

            Datasets.CollectionChanged += (_, __) => RefreshDerivedState();
            Warnings.CollectionChanged += (_, __) => RefreshDerivedState();

            NormalizationOptions.Add("None");
            NormalizationOptions.Add("Baseline voltage");
            NormalizationOptions.Add("Capacity");
            ResampleOptions.Add("Native");
            ResampleOptions.Add("1 s");
            ResampleOptions.Add("100 ms");

            SelectedNormalization = NormalizationOptions.FirstOrDefault();
            SelectedResampleOption = ResampleOptions.FirstOrDefault();

            ChartModes.Add(new ChartModeOption("Voltage", "ChartLine"));
            ChartModes.Add(new ChartModeOption("Current", "TrendingDown"));
            ChartModes.Add(new ChartModeOption("Temperature", "Thermometer"));
            ChartModes.Add(new ChartModeOption("Custom", "Tune"));
            SelectedChartMode = ChartModes.FirstOrDefault();

            AutomaticAnalyses.Add(new AnalysisOperationStatus(
                "ECM parameters",
                "Compute R0, R1, C1 and quality indicators from pulse segments",
                new RelayCommand(() => SimulateAnalysis("ECM parameters"))));
            AutomaticAnalyses.Add(new AnalysisOperationStatus(
                "Qmax",
                "Estimate Qmax from discharge curves with temperature adjustment",
                new RelayCommand(() => SimulateAnalysis("Qmax"))));
            AutomaticAnalyses.Add(new AnalysisOperationStatus(
                "LUT table",
                "Generate LUT entries for SOC vs. voltage for export",
                new RelayCommand(() => SimulateAnalysis("LUT table"))));

            LoadedSessionHistory.Add(new SessionSnapshot("Current session", DateTime.Now));
            LoadedSessionHistory.Add(new SessionSnapshot("Regression pack", DateTime.Now.AddDays(-2)));
            SelectedSessionSnapshot = LoadedSessionHistory.FirstOrDefault();

            BuildDesignTimeData();
            RefreshDerivedState();
        }

        /// <summary>Keeps the alignment hint text synchronized with the slider.</summary>
        partial void OnAlignmentOffsetChanged(double value)
        {
            BaselineAlignmentHint = $"{value:F1} s";
        }

        /// <summary>Creates a fake dataset entry and updates derived summary values.</summary>
        private void AddMockDataset()
        {
            var index = Datasets.Count + 1;
            Brush color;
            switch (index % 3)
            {
                case 1:
                    color = Brushes.SteelBlue;
                    break;
                case 2:
                    color = Brushes.IndianRed;
                    break;
                default:
                    color = Brushes.OliveDrab;
                    break;
            }

            var dataset = new AnalysisDataset($"Cycle {index}",
                $"Imported {DateTime.Now:yyyy-MM-dd HH:mm} • 4 channels • 10k samples",
                color)
            {
                ImportedAt = DateTime.Now,
                Notes = "Design time placeholder"
            };

            if (!Datasets.Any(d => d.IsBaseline))
            {
                dataset.IsBaseline = true;
            }

            Datasets.Add(dataset);
            dataset.PropertyChanged += OnDatasetPropertyChanged;
            SelectedDataset = dataset;
            UpdateStatisticsFor(dataset);
            RefreshDerivedState();
        }

        /// <summary>Updates statistics when the selected dataset changes.</summary>
        partial void OnSelectedDatasetChanged(AnalysisDataset? value)
        {
            if (value != null)
            {
                UpdateStatisticsFor(value);
            }
        }

        /// <summary>Removes the dataset from the list and ensures a baseline remains.</summary>
        private void RemoveDataset(AnalysisDataset dataset)
        {
            dataset.PropertyChanged -= OnDatasetPropertyChanged;
            Datasets.Remove(dataset);
            if (ReferenceEquals(SelectedDataset, dataset))
            {
                SelectedDataset = Datasets.FirstOrDefault();
            }

            if (!Datasets.Any(d => d.IsBaseline) && Datasets.FirstOrDefault() is { } first)
            {
                first.IsBaseline = true;
            }

            RefreshDerivedState();
        }

        /// <summary>Persists the current analysis configuration as a reusable template.</summary>
        private void SaveCurrentTemplate()
        {
            var template = new AnalysisTemplate(
                $"Template {SessionTemplates.Count + 1}",
                "Datasets, chart mode, and analysis parameters",
                DateTime.Now)
            {
                PreferredChartMode = SelectedChartMode?.Name ?? "Voltage",
                NormalizationOption = SelectedNormalization ?? "None",
                ResampleOption = SelectedResampleOption ?? "Native"
            };

            SessionTemplates.Add(template);
            AnalysisLog.Add($"Template '{template.Name}' saved");
        }

        /// <summary>Simulates an analysis workflow to populate UI state without backend logic.</summary>
        private void SimulateAnalysis(string analysisName)
        {
            var status = AutomaticAnalyses.FirstOrDefault(a => a.Title == analysisName);
            if (status == null)
            {
                return;
            }

            RemovePinnedResult(analysisName);

            status.IsBusy = true;
            status.Progress = 100;
            status.StatusMessage = "Completed";
            status.ErrorMessage = null;
            status.LastRunAt = DateTime.Now;
            status.LastRunText = status.LastRunAt?.ToString("yyyy-MM-dd HH:mm");
            status.IsBusy = false;

            PinnedResults.Add(new AnalysisResultSummary(
                analysisName,
                $"{analysisName} results • {DateTime.Now:HH:mm}",
                new RelayCommand(() => AnalysisLog.Add($"Opened {analysisName} result")),
                new RelayCommand(() => RemovePinnedResult(analysisName))));

            status.Progress = 0;

            LastAnalysisTimestampText = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            AnalysisLog.Add($"{analysisName} analysis finished");
        }

        /// <summary>Deletes pinned results so the panel does not accumulate duplicates.</summary>
        private void RemovePinnedResult(string analysisName)
        {
            var item = PinnedResults.FirstOrDefault(r => r.Title == analysisName);
            if (item != null)
            {
                PinnedResults.Remove(item);
                AnalysisLog.Add($"Removed pinned result '{analysisName}'");
            }
        }

        /// <summary>Seeds collections with realistic data for mock/demo scenarios.</summary>
        private void BuildDesignTimeData()
        {
            if (Datasets.Count == 0)
            {
                var primary = new AnalysisDataset("Cycle 1", "Imported 2024-05-11 09:15 • 4 channels • 12k samples", Brushes.SteelBlue)
                {
                    IsBaseline = true,
                    ImportedAt = DateTime.Now.AddDays(-1),
                    Notes = "Reference pulse run"
                };
                var secondary = new AnalysisDataset("Cycle 2", "Imported 2024-05-12 10:27 • 4 channels • 12k samples", Brushes.IndianRed)
                {
                    ImportedAt = DateTime.Now.AddHours(-4),
                    Notes = "Fresh measurement"
                };

                Datasets.Add(primary);
                Datasets.Add(secondary);
                primary.PropertyChanged += OnDatasetPropertyChanged;
                secondary.PropertyChanged += OnDatasetPropertyChanged;
                SelectedDataset = secondary;
            }

            if (ChartSeriesOptions.Count == 0)
            {
                ChartSeriesOptions.Add(new ComparisonSeriesOption("Voltage"));
                ChartSeriesOptions.Add(new ComparisonSeriesOption("Current"));
                ChartSeriesOptions.Add(new ComparisonSeriesOption("Temperature"));
                ChartSeriesOptions.Add(new ComparisonSeriesOption("Capacity"));
            }

            if (Warnings.Count == 0)
            {
                Warnings.Add(new AnalysisWarning("Pulse segment missing headers", "Verify RAW log between 120s and 180s."));
                Warnings.Add(new AnalysisWarning("Temperature drift detected", "Apply correction or exclude segment before ECM computation."));
            }

            if (SessionTemplates.Count == 0)
            {
                SessionTemplates.Add(new AnalysisTemplate("ECM regression", "Baseline + regression overlay", DateTime.Now.AddDays(-3))
                {
                    PreferredChartMode = "Voltage",
                    NormalizationOption = "Baseline voltage",
                    ResampleOption = "1 s"
                });
            }

            if (PinnedResults.Count == 0)
            {
                PinnedResults.Add(new AnalysisResultSummary(
                    "ECM parameters",
                    "R0 2.1 mΩ • Fit error 1.2%",
                    new RelayCommand(() => AnalysisLog.Add("Opened ECM parameters")),
                    new RelayCommand(() => RemovePinnedResult("ECM parameters"))));
            }

            LoadedFileCount = Datasets.Count;
            SampleRateStatus = "Aligned (1 s)";
            TotalTestDurationText = "1h 45m";
            VoltageSummary = "3.05 / 4.20 / 3.72 V";
            CurrentSummary = "-8.0 / 10.0 / 0.2 A";
            EnergySummary = "52.4 Wh";
            SelectionSummary = "Full dataset";
            SelectionVoltageSummary = "3.10 / 3.80 / 3.55 V";
            SelectionDurationText = "Span: 00:30:00";
            CurrentCellLabel = "Cell A1 • SN-4812";
        }

        /// <summary>Updates summary metrics displayed in the header area.</summary>
        private void UpdateStatisticsFor(AnalysisDataset dataset)
        {
            if (dataset == null)
            {
                return;
            }

            TotalTestDurationText = "1h 32m";
            VoltageSummary = "3.02 / 4.18 / 3.70 V";
            CurrentSummary = "-7.5 / 9.8 / 0.1 A";
            EnergySummary = "50.8 Wh";
            SelectionSummary = $"Viewing {dataset.DisplayName}";
            SelectionVoltageSummary = "3.05 / 3.80 / 3.60 V";
            SelectionDurationText = "Span: 00:20:00";
        }

        /// <summary>Ensures only one dataset is marked as baseline and logs inclusion changes.</summary>
        private void OnDatasetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not AnalysisDataset dataset)
            {
                return;
            }

            if (e.PropertyName == nameof(AnalysisDataset.IsBaseline) && dataset.IsBaseline)
            {
                foreach (var other in Datasets.Where(d => !ReferenceEquals(d, dataset)))
                {
                    other.IsBaseline = false;
                }
            }

            if (e.PropertyName == nameof(AnalysisDataset.IsIncluded))
            {
                AnalysisLog.Add($"{dataset.DisplayName} inclusion changed: {dataset.IsIncluded}");
            }
        }

        /// <summary>Updates lightweight counters that keep the UI badges in sync.</summary>
        private void RefreshDerivedState()
        {
            LoadedFileCount = Datasets.Count;
            HasWarnings = Warnings.Count > 0;
        }
    }

    /// <summary>Represents an analysed dataset including metadata and baseline flags.</summary>
    public partial class AnalysisDataset : ObservableObject
    {
        public AnalysisDataset(string displayName, string metadata, Brush seriesBrush)
        {
            DisplayName = displayName;
            Metadata = metadata;
            SeriesBrush = seriesBrush;
        }

        public string DisplayName { get; }
        public string Metadata { get; }
        public Brush SeriesBrush { get; }

        [ObservableProperty]
        private bool _isBaseline;

        [ObservableProperty]
        private bool _isIncluded = true;

        [ObservableProperty]
        private DateTime _importedAt;

        [ObservableProperty]
        private string? _notes;
    }

    /// <summary>Reusable configuration for applying analysis preferences to future sessions.</summary>
    public partial class AnalysisTemplate : ObservableObject
    {
        public AnalysisTemplate(string name, string description, DateTime createdAt)
        {
            Name = name;
            Description = description;
            CreatedAt = createdAt;
        }

        public string Name { get; }
        public string Description { get; }
        public DateTime CreatedAt { get; }

        [ObservableProperty]
        private string? _preferredChartMode;

        [ObservableProperty]
        private string? _normalizationOption;

        [ObservableProperty]
        private string? _resampleOption;
    }

    /// <summary>Captures an issue detected during analysis and a suggested follow-up action.</summary>
    public partial class AnalysisWarning : ObservableObject
    {
        public AnalysisWarning(string message, string suggestedAction)
        {
            Message = message;
            SuggestedAction = suggestedAction;
        }

        public string Message { get; }
        public string SuggestedAction { get; }
    }

    /// <summary>Selectable series that can be toggled on and off in the comparison chart.</summary>
    public partial class ComparisonSeriesOption : ObservableObject
    {
        public ComparisonSeriesOption(string name)
        {
            Name = name;
        }

        public string Name { get; }

        [ObservableProperty]
        private bool _isVisible = true;
    }

    /// <summary>Represents a chart layout option with a matching icon.</summary>
    public partial class ChartModeOption : ObservableObject
    {
        public ChartModeOption(string name, string icon)
        {
            Name = name;
            Icon = icon;
        }

        public string Name { get; }
        public string Icon { get; }
    }

    /// <summary>Tracks background analysis jobs and exposes progress for the UI.</summary>
    public partial class AnalysisOperationStatus : ObservableObject
    {
        public AnalysisOperationStatus(string title, string description, IRelayCommand recomputeCommand)
        {
            Title = title;
            Description = description;
            RecomputeCommand = recomputeCommand;
        }

        public string Title { get; }
        public string Description { get; }
        public IRelayCommand RecomputeCommand { get; }

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private string? _statusMessage = "Idle";

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private DateTime? _lastRunAt;

        [ObservableProperty]
        private string? _lastRunText;

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));
    }

    /// <summary>Summary tile for quick access to a stored analysis result.</summary>
    public partial class AnalysisResultSummary : ObservableObject
    {
        public AnalysisResultSummary(string title, string subtitle, IRelayCommand openCommand, IRelayCommand removeCommand)
        {
            Title = title;
            Subtitle = subtitle;
            OpenCommand = openCommand;
            RemoveCommand = removeCommand;
        }

        public string Title { get; }
        public string Subtitle { get; }
        public IRelayCommand OpenCommand { get; }
        public IRelayCommand RemoveCommand { get; }
    }

    /// <summary>Represents a saved analysis session that can be reloaded later.</summary>
    public partial class SessionSnapshot : ObservableObject
    {
        public SessionSnapshot(string title, DateTime savedAt)
        {
            Title = title;
            SavedAt = savedAt;
        }

        public string Title { get; }
        public DateTime SavedAt { get; }
        public override string ToString() => $"{Title} • {SavedAt:yyyy-MM-dd HH:mm}";
    }
}
