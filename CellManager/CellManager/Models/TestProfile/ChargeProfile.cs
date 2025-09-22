using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models.TestProfile
{
    /// <summary>
    ///     Stores the configuration of a charging step, including helpers for keeping split time inputs synchronized.
    /// </summary>
    public partial class ChargeProfile : ObservableObject
    {
        [ObservableProperty] private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _name;

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _chargeCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _chargeCutoffVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double _cutoffCurrent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private ChargeMode _chargeMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private double? _chargeCapacityMah;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        private TimeSpan _chargeTime;

        [ObservableProperty]
        private int _chargeHours;

        [ObservableProperty]
        private int _chargeMinutes;

        [ObservableProperty]
        private int _chargeSeconds;

        private bool _updatingChargeTime;

        partial void OnChargeHoursChanged(int value) => UpdateChargeTime();
        partial void OnChargeMinutesChanged(int value) => UpdateChargeTime();
        partial void OnChargeSecondsChanged(int value) => UpdateChargeTime();

        partial void OnChargeTimeChanged(TimeSpan value)
        {
            if (_updatingChargeTime) return;
            _updatingChargeTime = true;
            ChargeHours = value.Hours;
            ChargeMinutes = value.Minutes;
            ChargeSeconds = value.Seconds;
            _updatingChargeTime = false;
        }

        /// <summary>
        ///     Updates the aggregate <see cref="TimeSpan"/> when one of the component fields changes.
        /// </summary>
        private void UpdateChargeTime()
        {
            if (_updatingChargeTime) return;
            _updatingChargeTime = true;
            ChargeTime = new TimeSpan(ChargeHours, ChargeMinutes, ChargeSeconds);
            _updatingChargeTime = false;
        }

        /// <summary>
        ///     Text displayed in the UI profile tree summarizing the key charge settings.
        /// </summary>
        public string PreviewText
        {
            get
            {
                var baseText = $"{ChargeMode} , {ChargeCutoffVoltage} mV, {ChargeCurrent} mA, {CutoffCurrent} mA";
                return ChargeMode switch
                {
                    ChargeMode.ChargeByCapacity => $"{baseText}, {ChargeCapacityMah} mAh",
                    ChargeMode.ChargeByTime => $"{baseText}, {ChargeTime}",
                    _ => baseText
                };
            }
        }
    }

    /// <summary>
    ///     Available strategies for determining when to stop the charge step.
    /// </summary>
    public enum ChargeMode
    {
        ChargeByCapacity,
        ChargeByTime,
        FullCharge
    }
}
