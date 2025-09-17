using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CellManager.Models
{
    public partial class Schedule : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndScript))]
        private int _id;

        [ObservableProperty]
        private int _cellId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndScript))]
        private string _name;

        [ObservableProperty]
        private List<int> _testProfileIds = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndScript))]
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

        public string DisplayNameAndScript
        {
            get
            {
                var scriptNumber = Ordering > 0 ? Ordering : Id;
                return $"Script {scriptNumber} - {Name}";
            }
        }
    }
}