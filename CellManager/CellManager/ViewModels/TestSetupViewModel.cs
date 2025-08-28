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
using CellManager.ViewModels.TestSetup;

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

        private readonly ProfileManager<ChargeProfile> _chargeManager;
        private readonly ProfileManager<DischargeProfile> _dischargeManager;
        private readonly ProfileManager<ECMPulseProfile> _ecmManager;
        private readonly ProfileManager<OCVProfile> _ocvManager;
        private readonly ProfileManager<RestProfile> _restManager;

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
            _chargeManager = new ProfileManager<ChargeProfile>(
                ProfileKind.Charge,
                () => new ChargeProfile { Name = "New Charge" },
                () => ChargeProfiles,
                () => SelectedChargeProfile,
                p => SelectedChargeProfile = p,
                (p, id) => _chargeRepo.Save(p, id),
                p => _chargeRepo.Delete(p),
                p => p.Id,
                p => p.Name,
                () => SelectedCell?.Id ?? 0,
                ReloadAll,
                k => ActiveEditor = k,
                NotifyCanExecutes);

            _dischargeManager = new ProfileManager<DischargeProfile>(
                ProfileKind.Discharge,
                () => new DischargeProfile { Name = "New Discharge" },
                () => DischargeProfiles,
                () => SelectedDischargeProfile,
                p => SelectedDischargeProfile = p,
                (p, id) => _dischargeRepo.Save(p, id),
                p => _dischargeRepo.Delete(p),
                p => p.Id,
                p => p.Name,
                () => SelectedCell?.Id ?? 0,
                ReloadAll,
                k => ActiveEditor = k,
                NotifyCanExecutes);

            _ecmManager = new ProfileManager<ECMPulseProfile>(
                ProfileKind.Ecm,
                () => new ECMPulseProfile { Name = "New ECM" },
                () => EcmPulseProfiles,
                () => SelectedEcmPulseProfile,
                p => SelectedEcmPulseProfile = p,
                (p, id) => _ecmRepo.Save(p, id),
                p => _ecmRepo.Delete(p),
                p => p.Id,
                p => p.Name,
                () => SelectedCell?.Id ?? 0,
                ReloadAll,
                k => ActiveEditor = k,
                NotifyCanExecutes);

            _ocvManager = new ProfileManager<OCVProfile>(
                ProfileKind.Ocv,
                () => new OCVProfile { Name = "New OCV" },
                () => OcvProfiles,
                () => SelectedOcvProfile,
                p => SelectedOcvProfile = p,
                (p, id) => _ocvRepo.Save(p, id),
                p => _ocvRepo.Delete(p),
                p => p.Id,
                p => p.Name,
                () => SelectedCell?.Id ?? 0,
                ReloadAll,
                k => ActiveEditor = k,
                NotifyCanExecutes);

            _restManager = new ProfileManager<RestProfile>(
                ProfileKind.Rest,
                () => new RestProfile { Name = "New Rest" },
                () => RestProfiles,
                () => SelectedRestProfile,
                p => SelectedRestProfile = p,
                (p, id) => _restRepo.Save(p, id),
                p => _restRepo.Delete(p),
                p => p.Id,
                p => p.Name,
                () => SelectedCell?.Id ?? 0,
                ReloadAll,
                k => ActiveEditor = k,
                NotifyCanExecutes);

            // Create commands
            AddChargeProfileCommand = new RelayCommand(_chargeManager.Add);
            SaveChargeProfileCommand = new RelayCommand(_chargeManager.Save, () => SelectedChargeProfile != null && SelectedCell?.Id > 0);
            DeleteChargeProfileCommand = new RelayCommand(_chargeManager.Delete, () => SelectedChargeProfile != null);

            AddDischargeProfileCommand = new RelayCommand(_dischargeManager.Add);
            SaveDischargeProfileCommand = new RelayCommand(_dischargeManager.Save, () => SelectedDischargeProfile != null && SelectedCell?.Id > 0);
            DeleteDischargeProfileCommand = new RelayCommand(_dischargeManager.Delete, () => SelectedDischargeProfile != null);

            AddEcmProfileCommand = new RelayCommand(_ecmManager.Add);
            SaveEcmProfileCommand = new RelayCommand(_ecmManager.Save, () => SelectedEcmPulseProfile != null && SelectedCell?.Id > 0);
            DeleteEcmProfileCommand = new RelayCommand(_ecmManager.Delete, () => SelectedEcmPulseProfile != null);

            AddOcvProfileCommand = new RelayCommand(_ocvManager.Add);
            SaveOcvProfileCommand = new RelayCommand(_ocvManager.Save, () => SelectedOcvProfile != null && SelectedCell?.Id > 0);
            DeleteOcvProfileCommand = new RelayCommand(_ocvManager.Delete, () => SelectedOcvProfile != null);

            AddRestProfileCommand = new RelayCommand(_restManager.Add);
            SaveRestProfileCommand = new RelayCommand(_restManager.Save, () => SelectedRestProfile != null && SelectedCell?.Id > 0);
            DeleteRestProfileCommand = new RelayCommand(_restManager.Delete, () => SelectedRestProfile != null);

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
                case ProfileKind.Charge: _chargeManager.Save(); break;
                case ProfileKind.Discharge: _dischargeManager.Save(); break;
                case ProfileKind.Ecm: _ecmManager.Save(); break;
                case ProfileKind.Ocv: _ocvManager.Save(); break;
                case ProfileKind.Rest: _restManager.Save(); break;
            }
        }
        private void DeleteCurrent()
        {
            switch (ActiveEditor)
            {
                case ProfileKind.Charge: _chargeManager.Delete(); break;
                case ProfileKind.Discharge: _dischargeManager.Delete(); break;
                case ProfileKind.Ecm: _ecmManager.Delete(); break;
                case ProfileKind.Ocv: _ocvManager.Delete(); break;
                case ProfileKind.Rest: _restManager.Delete(); break;
            }
        }
        // CRUD operations are handled by ProfileManager instances
    }
}
