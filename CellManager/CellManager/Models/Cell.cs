using CommunityToolkit.Mvvm.ComponentModel;

namespace CellManager.Models
{
    public partial class Cell : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayNameAndId))]
        private string _modelName;
        [ObservableProperty]
        private string _manufacturer;
        [ObservableProperty]
        private string _serialNumber;
        [ObservableProperty]
        private string _partNumber;

        [ObservableProperty]
        private double _ratedCapacity;
        [ObservableProperty]
        private double _nominalVoltage;
        [ObservableProperty]
        private double _selfDischarge;
        [ObservableProperty]
        private double _maxVoltage;
        [ObservableProperty]
        private int _cycleLife;
        [ObservableProperty]
        private double _initialACImpedance;
        [ObservableProperty]
        private double _initialDCResistance;
        [ObservableProperty]
        private double _energyWh;
        [ObservableProperty]
        private string _cellType;
        [ObservableProperty]
        private double _weight;
        [ObservableProperty]
        private double _diameter;
        [ObservableProperty]
        private double _thickness;
        [ObservableProperty]
        private double _width;
        [ObservableProperty]
        private double _height;
        [ObservableProperty]
        private string _expansionBehavior;
        [ObservableProperty]
        private double _chargingVoltage;
        [ObservableProperty]
        private double _cutOffCurrent_Charge;
        [ObservableProperty]
        private double _maxChargingCurrent;
        [ObservableProperty]
        private double _maxChargingTemp;
        [ObservableProperty]
        private double _chargeTempHigh;
        [ObservableProperty]
        private double _chargeTempLow;
        [ObservableProperty]
        private double _dischargeCutOffVoltage;
        [ObservableProperty]
        private double _maxDischargingCurrent;
        [ObservableProperty]
        private double _dischargeTempHigh;
        [ObservableProperty]
        private double _dischargeTempLow;
        [ObservableProperty]
        private double _constantCurrent_PreCharge;
        [ObservableProperty]
        private double _preChargeStartVoltage;
        [ObservableProperty]
        private double _preChargeEndVoltage;

        [ObservableProperty]
        private bool _isActive = false;  

        public string DisplayNameAndId => $"ID: {Id} - {ModelName}";

        public Cell() { }
        public Cell(Cell source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Id = source.Id;
            ModelName = source.ModelName;
            Manufacturer = source.Manufacturer;
            SerialNumber = source.SerialNumber;
            PartNumber = source.PartNumber;
            RatedCapacity = source.RatedCapacity;
            NominalVoltage = source.NominalVoltage;
            SelfDischarge = source.SelfDischarge;
            MaxVoltage = source.MaxVoltage;
            CycleLife = source.CycleLife;
            InitialACImpedance = source.InitialACImpedance;
            InitialDCResistance = source.InitialDCResistance;
            EnergyWh = source.EnergyWh;
            CellType = source.CellType;
            Weight = source.Weight;
            Diameter = source.Diameter;
            Thickness = source.Thickness;
            Width = source.Width;
            Height = source.Height;
            ExpansionBehavior = source.ExpansionBehavior;
            ChargingVoltage = source.ChargingVoltage;
            CutOffCurrent_Charge = source.CutOffCurrent_Charge;
            MaxChargingCurrent = source.MaxChargingCurrent;
            MaxChargingTemp = source.MaxChargingTemp;
            ChargeTempHigh = source.ChargeTempHigh;
            ChargeTempLow = source.ChargeTempLow;
            DischargeCutOffVoltage = source.DischargeCutOffVoltage;
            MaxDischargingCurrent = source.MaxDischargingCurrent;
            DischargeTempHigh = source.DischargeTempHigh;
            DischargeTempLow = source.DischargeTempLow;
            ConstantCurrent_PreCharge = source.ConstantCurrent_PreCharge;
            PreChargeStartVoltage = source.PreChargeStartVoltage;
            PreChargeEndVoltage = source.PreChargeEndVoltage;
            IsActive = source.IsActive;
        }
        public void CopyFrom(Cell source)
        {
            Id = source.Id;
            ModelName = source.ModelName;
            Manufacturer = source.Manufacturer;
            SerialNumber = source.SerialNumber;
            PartNumber = source.PartNumber;
            RatedCapacity = source.RatedCapacity;
            NominalVoltage = source.NominalVoltage;
            SelfDischarge = source.SelfDischarge;
            MaxVoltage = source.MaxVoltage;
            CycleLife = source.CycleLife;
            InitialACImpedance = source.InitialACImpedance;
            InitialDCResistance = source.InitialDCResistance;
            EnergyWh = source.EnergyWh;
            CellType = source.CellType;
            Weight = source.Weight;
            Diameter = source.Diameter;
            Thickness = source.Thickness;
            Width = source.Width;
            Height = source.Height;
            ExpansionBehavior = source.ExpansionBehavior;
            ChargingVoltage = source.ChargingVoltage;
            CutOffCurrent_Charge = source.CutOffCurrent_Charge;
            MaxChargingCurrent = source.MaxChargingCurrent;
            MaxChargingTemp = source.MaxChargingTemp;
            ChargeTempHigh = source.ChargeTempHigh;
            ChargeTempLow = source.ChargeTempLow;
            DischargeCutOffVoltage = source.DischargeCutOffVoltage;
            MaxDischargingCurrent = source.MaxDischargingCurrent;
            DischargeTempHigh = source.DischargeTempHigh;
            DischargeTempLow = source.DischargeTempLow;
            ConstantCurrent_PreCharge = source.ConstantCurrent_PreCharge;
            PreChargeStartVoltage = source.PreChargeStartVoltage;
            PreChargeEndVoltage = source.PreChargeEndVoltage;
            IsActive = source.IsActive;
        }
    }
}