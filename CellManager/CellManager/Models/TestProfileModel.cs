using CommunityToolkit.Mvvm.ComponentModel;
using CellManager.Models.TestProfile;
using System;

namespace CellManager.Models
{
    /// <summary>
    ///     Aggregates the various profile subtypes so they can be persisted and edited as a unit.
    /// </summary>
    public partial class TestProfileModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _profileName;

        [ObservableProperty]
        private int _cellId;

        [ObservableProperty]
        private TestProfileType _profileType;

        /// <summary>Human-friendly identifier used by list controls.</summary>
        public string DisplayNameAndId => $"ID: {Id} - {ProfileName}";

        [ObservableProperty]
        private ChargeProfile _chargeProfile = new ChargeProfile();

        [ObservableProperty]
        private DischargeProfile _dischargeProfile = new DischargeProfile();

        [ObservableProperty]
        private RestProfile _restProfile = new RestProfile();

        [ObservableProperty]
        private OCVProfile _ocvProfile = new OCVProfile();

        [ObservableProperty]
        private ECMPulseProfile _ecmPulseProfile = new ECMPulseProfile();
    }
}