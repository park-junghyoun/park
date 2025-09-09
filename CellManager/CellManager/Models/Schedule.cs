using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CellManager.Models
{
    public partial class Schedule : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        [ObservableProperty]
        private List<int> _testProfileIds = new();

        [ObservableProperty]
        private int _ordering;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private int _repeatCount = 1;

        [ObservableProperty]
        private int _loopStartIndex;

        [ObservableProperty]
        private int _loopEndIndex;

        [ObservableProperty]
        private TimeSpan _estimatedDuration;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";
    }
}