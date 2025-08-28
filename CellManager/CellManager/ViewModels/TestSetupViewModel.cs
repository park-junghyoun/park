using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Messages;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Services;

namespace CellManager.ViewModels
{
    public enum ProfileKind { None, Charge, Discharge, Ecm, Ocv, Rest }

    public partial class TestSetupViewModel : ObservableObject
    {
        // --- Repositories ---
        private readonly IChargeProfileRepository _chargeRepo;
        private readonly IDischargeProfileRepository _dischargeRepo;
        private readonly IEcmPulseProfileRepository _ecmRepo;
        private readonly IOcvProfileRepository _ocvRepo;
        private readonly IRestProfileRepository _restRepo;

        // --- Tab header (for MainWindow TabControl) ---
        public string HeaderText { get; } = "Test Setup";
        public string IconName { get; } = "Tuning";
        [ObservableProperty] private bool _isViewEnabled = true;

        // --- Selection context ---
        [ObservableProperty] private Cell _selectedCell;
        partial void OnSelectedCellChanged(Cell value)
        {
            NotifyCanExecutes();
        }

        // Active editor type & current editor object (auto-changed by selection)
        [ObservableProperty] private ProfileKind _activeEditor = ProfileKind.None;
        public object CurrentEditor => ActiveEditor switch
        {
            ProfileKind.Charge => SelectedChargeProfile,
            ProfileKind.Discharge => SelectedDischargeProfile,
            ProfileKind.Ecm => SelectedEcmPulseProfile,
            ProfileKind.Ocv => SelectedOcvProfile,
            ProfileKind.Rest => SelectedRestProfile,
            _ => null
        };
        partial void OnActiveEditorChanged(ProfileKind value)
        {
            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        // --- Collections ---
        [ObservableProperty] private ObservableCollection<ChargeProfile> _chargeProfiles = new();
        [ObservableProperty] private ObservableCollection<DischargeProfile> _dischargeProfiles = new();
        [ObservableProperty] private ObservableCollection<ECMPulseProfile> _ecmPulseProfiles = new();
        [ObservableProperty] private ObservableCollection<OCVProfile> _ocvProfiles = new();
        [ObservableProperty] private ObservableCollection<RestProfile> _restProfiles = new();

        // --- Selected items (change sets ActiveEditor and clears others) ---
        [ObservableProperty] private ChargeProfile _selectedChargeProfile;
        partial void OnSelectedChargeProfileChanged(ChargeProfile value)
        {
            if (value != null)
            {
                ActiveEditor = ProfileKind.Charge;
                SelectedDischargeProfile = null;
                SelectedEcmPulseProfile = null;
                SelectedOcvProfile = null;
                SelectedRestProfile = null;
            }
            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        [ObservableProperty] private DischargeProfile _selectedDischargeProfile;
        partial void OnSelectedDischargeProfileChanged(DischargeProfile value)
        {
            if (value != null)
            {
                ActiveEditor = ProfileKind.Discharge;
                SelectedChargeProfile = null;
                SelectedEcmPulseProfile = null;
                SelectedOcvProfile = null;
                SelectedRestProfile = null;
            }
            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        [ObservableProperty] private ECMPulseProfile _selectedEcmPulseProfile;
        partial void OnSelectedEcmPulseProfileChanged(ECMPulseProfile value)
        {
            if (value != null)
            {
                ActiveEditor = ProfileKind.Ecm;
                SelectedChargeProfile = null;
                SelectedDischargeProfile = null;
                SelectedOcvProfile = null;
                SelectedRestProfile = null;
            }
            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        [ObservableProperty] private OCVProfile _selectedOcvProfile;
        partial void OnSelectedOcvProfileChanged(OCVProfile value)
        {
            if (value != null)
            {
                ActiveEditor = ProfileKind.Ocv;
                SelectedChargeProfile = null;
                SelectedDischargeProfile = null;
                SelectedEcmPulseProfile = null;
                SelectedRestProfile = null;
            }
            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        [ObservableProperty] private RestProfile _selectedRestProfile;
        partial void OnSelectedRestProfileChanged(RestProfile value)
        {
            if (value != null)
            {
                ActiveEditor = ProfileKind.Rest;
                SelectedChargeProfile = null;
                SelectedDischargeProfile = null;
                SelectedEcmPulseProfile = null;
                SelectedOcvProfile = null;
            }
            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        // --- Commands (typed as RelayCommand for direct NotifyCanExecuteChanged) ---
        public RelayCommand AddChargeProfileCommand { get; }
        public RelayCommand SaveChargeProfileCommand { get; }
        public RelayCommand DeleteChargeProfileCommand { get; }

        public RelayCommand AddDischargeProfileCommand { get; }
        public RelayCommand SaveDischargeProfileCommand { get; }
        public RelayCommand DeleteDischargeProfileCommand { get; }

        public RelayCommand AddEcmProfileCommand { get; }
        public RelayCommand SaveEcmProfileCommand { get; }
        public RelayCommand DeleteEcmProfileCommand { get; }

        public RelayCommand AddOcvProfileCommand { get; }
        public RelayCommand SaveOcvProfileCommand { get; }
        public RelayCommand DeleteOcvProfileCommand { get; }

        public RelayCommand AddRestProfileCommand { get; }
        public RelayCommand SaveRestProfileCommand { get; }
        public RelayCommand DeleteRestProfileCommand { get; }

        // Generic buttons on the right pane
        public RelayCommand SaveCurrentCommand { get; }
        public RelayCommand DeleteCurrentCommand { get; }

        public TestSetupViewModel(
            IChargeProfileRepository chargeRepo,
            IDischargeProfileRepository dischargeRepo,
            IEcmPulseProfileRepository ecmRepo,
            IOcvProfileRepository ocvRepo,
            IRestProfileRepository restRepo
        )
        {
            _chargeRepo = chargeRepo;
            _dischargeRepo = dischargeRepo;
            _ecmRepo = ecmRepo;
            _ocvRepo = ocvRepo;
            _restRepo = restRepo;

            // Create commands
            AddChargeProfileCommand = new RelayCommand(AddCharge, () => SelectedCell?.Id > 0);
            SaveChargeProfileCommand = new RelayCommand(SaveCharge, () => SelectedChargeProfile != null && SelectedCell?.Id > 0);
            DeleteChargeProfileCommand = new RelayCommand(DeleteCharge, () => SelectedChargeProfile != null);

            AddDischargeProfileCommand = new RelayCommand(AddDischarge, () => SelectedCell?.Id > 0);
            SaveDischargeProfileCommand = new RelayCommand(SaveDischarge, () => SelectedDischargeProfile != null && SelectedCell?.Id > 0);
            DeleteDischargeProfileCommand = new RelayCommand(DeleteDischarge, () => SelectedDischargeProfile != null);

            AddEcmProfileCommand = new RelayCommand(AddEcm, () => SelectedCell?.Id > 0);
            SaveEcmProfileCommand = new RelayCommand(SaveEcm, () => SelectedEcmPulseProfile != null && SelectedCell?.Id > 0);
            DeleteEcmProfileCommand = new RelayCommand(DeleteEcm, () => SelectedEcmPulseProfile != null);

            AddOcvProfileCommand = new RelayCommand(AddOcv, () => SelectedCell?.Id > 0);
            SaveOcvProfileCommand = new RelayCommand(SaveOcv, () => SelectedOcvProfile != null && SelectedCell?.Id > 0);
            DeleteOcvProfileCommand = new RelayCommand(DeleteOcv, () => SelectedOcvProfile != null);

            AddRestProfileCommand = new RelayCommand(AddRest, () => SelectedCell?.Id > 0);
            SaveRestProfileCommand = new RelayCommand(SaveRest, () => SelectedRestProfile != null && SelectedCell?.Id > 0);
            DeleteRestProfileCommand = new RelayCommand(DeleteRest, () => SelectedRestProfile != null);

            SaveCurrentCommand = new RelayCommand(SaveCurrent, CanSaveCurrent);
            DeleteCurrentCommand = new RelayCommand(DeleteCurrent, CanDeleteCurrent);

            // When cell is selected in library → load profiles
            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                SelectedCell = m.SelectedCell;
                ReloadAll();
            });
        }

        // --- Helpers ---
        private void NotifyCanExecutes()
        {
            AddChargeProfileCommand.NotifyCanExecuteChanged();
            SaveChargeProfileCommand.NotifyCanExecuteChanged();
            DeleteChargeProfileCommand.NotifyCanExecuteChanged();
            AddDischargeProfileCommand.NotifyCanExecuteChanged();
            SaveDischargeProfileCommand.NotifyCanExecuteChanged();
            DeleteDischargeProfileCommand.NotifyCanExecuteChanged();
            AddEcmProfileCommand.NotifyCanExecuteChanged();
            SaveEcmProfileCommand.NotifyCanExecuteChanged();
            DeleteEcmProfileCommand.NotifyCanExecuteChanged();
            AddOcvProfileCommand.NotifyCanExecuteChanged();
            SaveOcvProfileCommand.NotifyCanExecuteChanged();
            DeleteOcvProfileCommand.NotifyCanExecuteChanged();
            AddRestProfileCommand.NotifyCanExecuteChanged();
            SaveRestProfileCommand.NotifyCanExecuteChanged();
            DeleteRestProfileCommand.NotifyCanExecuteChanged();
            SaveCurrentCommand.NotifyCanExecuteChanged();
            DeleteCurrentCommand.NotifyCanExecuteChanged();
        }

        private void ReloadAll()
        {
            if (SelectedCell?.Id > 0)
            {
                ChargeProfiles = _chargeRepo.Load(SelectedCell.Id);
                DischargeProfiles = _dischargeRepo.Load(SelectedCell.Id);
                EcmPulseProfiles = _ecmRepo.Load(SelectedCell.Id);
                OcvProfiles = _ocvRepo.Load(SelectedCell.Id);
                RestProfiles = _restRepo.Load(SelectedCell.Id);
            }
            else
            {
                ChargeProfiles.Clear();
                DischargeProfiles.Clear();
                EcmPulseProfiles.Clear();
                OcvProfiles.Clear();
                RestProfiles.Clear();

                SelectedChargeProfile = null;
                SelectedDischargeProfile = null;
                SelectedEcmPulseProfile = null;
                SelectedOcvProfile = null;
                SelectedRestProfile = null;
                ActiveEditor = ProfileKind.None;
            }

            // Pick a reasonable editor if none
            if (ActiveEditor == ProfileKind.None)
            {
                if (ChargeProfiles?.Any() == true) ActiveEditor = ProfileKind.Charge;
                else if (DischargeProfiles?.Any() == true) ActiveEditor = ProfileKind.Discharge;
                else if (EcmPulseProfiles?.Any() == true) ActiveEditor = ProfileKind.Ecm;
                else if (OcvProfiles?.Any() == true) ActiveEditor = ProfileKind.Ocv;
                else if (RestProfiles?.Any() == true) ActiveEditor = ProfileKind.Rest;
            }

            OnPropertyChanged(nameof(CurrentEditor));
            NotifyCanExecutes();
        }

        // --- Generic right-pane buttons ---
        private bool CanSaveCurrent() =>
            (ActiveEditor == ProfileKind.Charge && SelectedChargeProfile != null && SelectedCell?.Id > 0) ||
            (ActiveEditor == ProfileKind.Discharge && SelectedDischargeProfile != null && SelectedCell?.Id > 0) ||
            (ActiveEditor == ProfileKind.Ecm && SelectedEcmPulseProfile != null && SelectedCell?.Id > 0) ||
            (ActiveEditor == ProfileKind.Ocv && SelectedOcvProfile != null && SelectedCell?.Id > 0) ||
            (ActiveEditor == ProfileKind.Rest && SelectedRestProfile != null && SelectedCell?.Id > 0);

        private bool CanDeleteCurrent() =>
            (ActiveEditor == ProfileKind.Charge && SelectedChargeProfile != null) ||
            (ActiveEditor == ProfileKind.Discharge && SelectedDischargeProfile != null) ||
            (ActiveEditor == ProfileKind.Ecm && SelectedEcmPulseProfile != null) ||
            (ActiveEditor == ProfileKind.Ocv && SelectedOcvProfile != null) ||
            (ActiveEditor == ProfileKind.Rest && SelectedRestProfile != null);

        private void SaveCurrent()
        {
            switch (ActiveEditor)
            {
                case ProfileKind.Charge: SaveCharge(); break;
                case ProfileKind.Discharge: SaveDischarge(); break;
                case ProfileKind.Ecm: SaveEcm(); break;
                case ProfileKind.Ocv: SaveOcv(); break;
                case ProfileKind.Rest: SaveRest(); break;
            }
        }
        private void DeleteCurrent()
        {
            switch (ActiveEditor)
            {
                case ProfileKind.Charge: DeleteCharge(); break;
                case ProfileKind.Discharge: DeleteDischarge(); break;
                case ProfileKind.Ecm: DeleteEcm(); break;
                case ProfileKind.Ocv: DeleteOcv(); break;
                case ProfileKind.Rest: DeleteRest(); break;
            }
        }

        // --- CRUD (safe reselect logic) ---
        private void AddCharge()
        {
            if (SelectedCell?.Id <= 0) return;
            SelectedChargeProfile = new ChargeProfile { Name = "New Charge" };
            ChargeProfiles.Add(SelectedChargeProfile);
            ActiveEditor = ProfileKind.Charge;
            NotifyCanExecutes();
        }
        private void SaveCharge()
        {
            var prevId = SelectedChargeProfile?.Id ?? 0;
            var prevName = SelectedChargeProfile?.Name;
            _chargeRepo.Save(SelectedChargeProfile, SelectedCell.Id);
            ReloadAll();
            if (prevId > 0) SelectedChargeProfile = ChargeProfiles.FirstOrDefault(p => p.Id == prevId);
            if (SelectedChargeProfile == null && !string.IsNullOrWhiteSpace(prevName))
                SelectedChargeProfile = ChargeProfiles.LastOrDefault(p => p.Name == prevName);
            if (SelectedChargeProfile == null) SelectedChargeProfile = ChargeProfiles.LastOrDefault();
            ActiveEditor = ProfileKind.Charge;
            NotifyCanExecutes();
        }
        private void DeleteCharge()
        {
            if (SelectedChargeProfile == null) return;
            _chargeRepo.Delete(SelectedChargeProfile);
            ReloadAll();
            ActiveEditor = ProfileKind.Charge;
        }

        private void AddDischarge()
        {
            if (SelectedCell?.Id <= 0) return;
            SelectedDischargeProfile = new DischargeProfile { Name = "New Discharge" };
            DischargeProfiles.Add(SelectedDischargeProfile);
            ActiveEditor = ProfileKind.Discharge;
            NotifyCanExecutes();
        }
        private void SaveDischarge()
        {
            var prevId = SelectedDischargeProfile?.Id ?? 0;
            var prevName = SelectedDischargeProfile?.Name;
            _dischargeRepo.Save(SelectedDischargeProfile, SelectedCell.Id);
            ReloadAll();
            if (prevId > 0) SelectedDischargeProfile = DischargeProfiles.FirstOrDefault(p => p.Id == prevId);
            if (SelectedDischargeProfile == null && !string.IsNullOrWhiteSpace(prevName))
                SelectedDischargeProfile = DischargeProfiles.LastOrDefault(p => p.Name == prevName);
            if (SelectedDischargeProfile == null) SelectedDischargeProfile = DischargeProfiles.LastOrDefault();
            ActiveEditor = ProfileKind.Discharge;
            NotifyCanExecutes();
        }
        private void DeleteDischarge()
        {
            if (SelectedDischargeProfile == null) return;
            _dischargeRepo.Delete(SelectedDischargeProfile);
            ReloadAll();
            ActiveEditor = ProfileKind.Discharge;
        }

        private void AddEcm()
        {
            if (SelectedCell?.Id <= 0) return;
            SelectedEcmPulseProfile = new ECMPulseProfile { Name = "New ECM" };
            EcmPulseProfiles.Add(SelectedEcmPulseProfile);
            ActiveEditor = ProfileKind.Ecm;
            NotifyCanExecutes();
        }
        private void SaveEcm()
        {
            var prevId = SelectedEcmPulseProfile?.Id ?? 0;
            var prevName = SelectedEcmPulseProfile?.Name;
            _ecmRepo.Save(SelectedEcmPulseProfile, SelectedCell.Id);
            ReloadAll();
            if (prevId > 0) SelectedEcmPulseProfile = EcmPulseProfiles.FirstOrDefault(p => p.Id == prevId);
            if (SelectedEcmPulseProfile == null && !string.IsNullOrWhiteSpace(prevName))
                SelectedEcmPulseProfile = EcmPulseProfiles.LastOrDefault(p => p.Name == prevName);
            if (SelectedEcmPulseProfile == null) SelectedEcmPulseProfile = EcmPulseProfiles.LastOrDefault();
            ActiveEditor = ProfileKind.Ecm;
            NotifyCanExecutes();
        }
        private void DeleteEcm()
        {
            if (SelectedEcmPulseProfile == null) return;
            _ecmRepo.Delete(SelectedEcmPulseProfile);
            ReloadAll();
            ActiveEditor = ProfileKind.Ecm;
        }

        private void AddOcv()
        {
            if (SelectedCell?.Id <= 0) return;
            SelectedOcvProfile = new OCVProfile { Name = "New OCV" };
            OcvProfiles.Add(SelectedOcvProfile);
            ActiveEditor = ProfileKind.Ocv;
            NotifyCanExecutes();
        }
        private void SaveOcv()
        {
            var prevId = SelectedOcvProfile?.Id ?? 0;
            var prevName = SelectedOcvProfile?.Name;
            _ocvRepo.Save(SelectedOcvProfile, SelectedCell.Id);
            ReloadAll();
            if (prevId > 0) SelectedOcvProfile = OcvProfiles.FirstOrDefault(p => p.Id == prevId);
            if (SelectedOcvProfile == null && !string.IsNullOrWhiteSpace(prevName))
                SelectedOcvProfile = OcvProfiles.LastOrDefault(p => p.Name == prevName);
            if (SelectedOcvProfile == null) SelectedOcvProfile = OcvProfiles.LastOrDefault();
            ActiveEditor = ProfileKind.Ocv;
            NotifyCanExecutes();
        }
        private void DeleteOcv()
        {
            if (SelectedOcvProfile == null) return;
            _ocvRepo.Delete(SelectedOcvProfile);
            ReloadAll();
            ActiveEditor = ProfileKind.Ocv;
        }

        private void AddRest()
        {
            if (SelectedCell?.Id <= 0) return;
            SelectedRestProfile = new RestProfile { Name = "New Rest" };
            RestProfiles.Add(SelectedRestProfile);
            ActiveEditor = ProfileKind.Rest;
            NotifyCanExecutes();
        }
        private void SaveRest()
        {
            var prevId = SelectedRestProfile?.Id ?? 0;
            var prevName = SelectedRestProfile?.Name;
            _restRepo.Save(SelectedRestProfile, SelectedCell.Id);
            ReloadAll();
            if (prevId > 0) SelectedRestProfile = RestProfiles.FirstOrDefault(p => p.Id == prevId);
            if (SelectedRestProfile == null && !string.IsNullOrWhiteSpace(prevName))
                SelectedRestProfile = RestProfiles.LastOrDefault(p => p.Name == prevName);
            if (SelectedRestProfile == null) SelectedRestProfile = RestProfiles.LastOrDefault();
            ActiveEditor = ProfileKind.Rest;
            NotifyCanExecutes();
        }
        private void DeleteRest()
        {
            if (SelectedRestProfile == null) return;
            _restRepo.Delete(SelectedRestProfile);
            ReloadAll();
            ActiveEditor = ProfileKind.Rest;
        }
    }
}