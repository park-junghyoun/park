using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Messages;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Services;
using CellManager.Views.TestSetup;

namespace CellManager.ViewModels
{
    public partial class TestSetupViewModel : ObservableObject
    {
        private readonly IChargeProfileRepository _chargeRepo;
        private readonly IDischargeProfileRepository _dischargeRepo;
        private readonly IRestProfileRepository _restRepo;
        private readonly IOcvProfileRepository _ocvRepo;
        private readonly IEcmPulseProfileRepository _ecmRepo;

        public string HeaderText { get; } = "Test Setup";
        public string IconName { get; } = "FileSign";
        [ObservableProperty] private bool _isViewEnabled = true;

        [ObservableProperty] private ObservableCollection<ChargeProfile> _chargeProfiles = new();
        [ObservableProperty] private ObservableCollection<DischargeProfile> _dischargeProfiles = new();
        [ObservableProperty] private ObservableCollection<RestProfile> _restProfiles = new();
        [ObservableProperty] private ObservableCollection<OCVProfile> _ocvProfiles = new();
        [ObservableProperty] private ObservableCollection<ECMPulseProfile> _ecmPulseProfiles = new();

        [ObservableProperty] private Cell _selectedCell;
        [ObservableProperty] private ChargeProfile _selectedChargeProfile;
        [ObservableProperty] private DischargeProfile _selectedDischargeProfile;
        [ObservableProperty] private RestProfile _selectedRestProfile;
        [ObservableProperty] private OCVProfile _selectedOcvProfile;
        [ObservableProperty] private ECMPulseProfile _selectedEcmPulseProfile;

        public RelayCommand AddChargeProfileCommand { get; }
        public RelayCommand<ChargeProfile> EditChargeProfileCommand { get; }
        public RelayCommand<ChargeProfile> DeleteChargeProfileCommand { get; }

        public RelayCommand AddDischargeProfileCommand { get; }
        public RelayCommand<DischargeProfile> EditDischargeProfileCommand { get; }
        public RelayCommand<DischargeProfile> DeleteDischargeProfileCommand { get; }

        public RelayCommand AddRestProfileCommand { get; }
        public RelayCommand<RestProfile> EditRestProfileCommand { get; }
        public RelayCommand<RestProfile> DeleteRestProfileCommand { get; }

        public RelayCommand AddOcvProfileCommand { get; }
        public RelayCommand<OCVProfile> EditOcvProfileCommand { get; }
        public RelayCommand<OCVProfile> DeleteOcvProfileCommand { get; }

        public RelayCommand AddEcmProfileCommand { get; }
        public RelayCommand<ECMPulseProfile> EditEcmProfileCommand { get; }
        public RelayCommand<ECMPulseProfile> DeleteEcmProfileCommand { get; }

        public TestSetupViewModel(
            IChargeProfileRepository chargeRepo,
            IDischargeProfileRepository dischargeRepo,
            IEcmPulseProfileRepository ecmRepo,
            IOcvProfileRepository ocvRepo,
            IRestProfileRepository restRepo)
        {
            _chargeRepo = chargeRepo;
            _dischargeRepo = dischargeRepo;
            _ecmRepo = ecmRepo;
            _ocvRepo = ocvRepo;
            _restRepo = restRepo;

            AddChargeProfileCommand = new RelayCommand(AddChargeProfile, () => SelectedCell?.Id > 0);
            EditChargeProfileCommand = new RelayCommand<ChargeProfile>(EditChargeProfile);
            DeleteChargeProfileCommand = new RelayCommand<ChargeProfile>(DeleteChargeProfile);

            AddDischargeProfileCommand = new RelayCommand(AddDischargeProfile, () => SelectedCell?.Id > 0);
            EditDischargeProfileCommand = new RelayCommand<DischargeProfile>(EditDischargeProfile);
            DeleteDischargeProfileCommand = new RelayCommand<DischargeProfile>(DeleteDischargeProfile);

            AddRestProfileCommand = new RelayCommand(AddRestProfile, () => SelectedCell?.Id > 0);
            EditRestProfileCommand = new RelayCommand<RestProfile>(EditRestProfile);
            DeleteRestProfileCommand = new RelayCommand<RestProfile>(DeleteRestProfile);

            AddOcvProfileCommand = new RelayCommand(AddOcvProfile, () => SelectedCell?.Id > 0);
            EditOcvProfileCommand = new RelayCommand<OCVProfile>(EditOcvProfile);
            DeleteOcvProfileCommand = new RelayCommand<OCVProfile>(DeleteOcvProfile);

            AddEcmProfileCommand = new RelayCommand(AddEcmProfile, () => SelectedCell?.Id > 0);
            EditEcmProfileCommand = new RelayCommand<ECMPulseProfile>(EditEcmProfile);
            DeleteEcmProfileCommand = new RelayCommand<ECMPulseProfile>(DeleteEcmProfile);

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                SelectedCell = m.SelectedCell;
                ReloadAll();
                UpdateCanExecutes();
            });
        }

        partial void OnSelectedCellChanged(Cell value) => UpdateCanExecutes();

        private void UpdateCanExecutes()
        {
            AddChargeProfileCommand.NotifyCanExecuteChanged();
            AddDischargeProfileCommand.NotifyCanExecuteChanged();
            AddRestProfileCommand.NotifyCanExecuteChanged();
            AddOcvProfileCommand.NotifyCanExecuteChanged();
            AddEcmProfileCommand.NotifyCanExecuteChanged();
        }

        private void AddChargeProfile()
        {
            var profile = new ChargeProfile { Name = "New Charge" };
            if (OpenEditor(profile))
            {
                _chargeRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void EditChargeProfile(ChargeProfile profile)
        {
            if (profile == null) return;
            var copy = Clone(profile);
            if (OpenEditor(copy))
            {
                _chargeRepo.Save(copy, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void DeleteChargeProfile(ChargeProfile profile)
        {
            if (profile == null) return;
            _chargeRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        private void AddDischargeProfile()
        {
            var profile = new DischargeProfile { Name = "New Discharge" };
            if (OpenEditor(profile))
            {
                _dischargeRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void EditDischargeProfile(DischargeProfile profile)
        {
            if (profile == null) return;
            var copy = Clone(profile);
            if (OpenEditor(copy))
            {
                _dischargeRepo.Save(copy, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void DeleteDischargeProfile(DischargeProfile profile)
        {
            if (profile == null) return;
            _dischargeRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        private void AddRestProfile()
        {
            var profile = new RestProfile { Name = "New Rest" };
            if (OpenEditor(profile))
            {
                _restRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void EditRestProfile(RestProfile profile)
        {
            if (profile == null) return;
            var copy = Clone(profile);
            if (OpenEditor(copy))
            {
                _restRepo.Save(copy, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void DeleteRestProfile(RestProfile profile)
        {
            if (profile == null) return;
            _restRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        private void AddOcvProfile()
        {
            var profile = new OCVProfile { Name = "New OCV" };
            if (OpenEditor(profile))
            {
                _ocvRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void EditOcvProfile(OCVProfile profile)
        {
            if (profile == null) return;
            var copy = Clone(profile);
            if (OpenEditor(copy))
            {
                _ocvRepo.Save(copy, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void DeleteOcvProfile(OCVProfile profile)
        {
            if (profile == null) return;
            _ocvRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        private void AddEcmProfile()
        {
            var profile = new ECMPulseProfile { Name = "New ECM" };
            if (OpenEditor(profile))
            {
                _ecmRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void EditEcmProfile(ECMPulseProfile profile)
        {
            if (profile == null) return;
            var copy = Clone(profile);
            if (OpenEditor(copy))
            {
                _ecmRepo.Save(copy, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        private void DeleteEcmProfile(ECMPulseProfile profile)
        {
            if (profile == null) return;
            _ecmRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        private static T Clone<T>(T source) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source))!;

        private int GetNextProfileId()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            var dbPath = Path.Combine(dataDir, "test_profiles.db");
            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();
            return ProfileIdProvider.GetNextId(conn);
        }

        private bool OpenEditor(object profile)
        {
            switch (profile)
            {
                case ChargeProfile cp:
                    cp.DisplayId = cp.Id > 0 ? cp.Id : GetNextProfileId();
                    break;
                case DischargeProfile dp:
                    dp.DisplayId = dp.Id > 0 ? dp.Id : GetNextProfileId();
                    break;
                case RestProfile rp:
                    rp.DisplayId = rp.Id > 0 ? rp.Id : GetNextProfileId();
                    break;
                case OCVProfile op:
                    op.DisplayId = op.Id > 0 ? op.Id : GetNextProfileId();
                    break;
                case ECMPulseProfile ep:
                    ep.DisplayId = ep.Id > 0 ? ep.Id : GetNextProfileId();
                    break;
            }

            var window = new ProfileDetailWindow { DataContext = profile };
            return window.ShowDialog() == true;
        }

        private void ReloadAll()
        {
            if (SelectedCell?.Id > 0)
            {
                ChargeProfiles = _chargeRepo.Load(SelectedCell.Id);
                DischargeProfiles = _dischargeRepo.Load(SelectedCell.Id);
                RestProfiles = _restRepo.Load(SelectedCell.Id);
                OcvProfiles = _ocvRepo.Load(SelectedCell.Id);
                EcmPulseProfiles = _ecmRepo.Load(SelectedCell.Id);
            }
            else
            {
                ChargeProfiles.Clear();
                DischargeProfiles.Clear();
                RestProfiles.Clear();
                OcvProfiles.Clear();
                EcmPulseProfiles.Clear();
            }
        }

        private void ReloadAllAndNotify()
        {
            ReloadAll();
            NotifyProfilesUpdated();
        }

        private void NotifyProfilesUpdated()
        {
            if (SelectedCell?.Id > 0)
                WeakReferenceMessenger.Default.Send(new TestProfilesUpdatedMessage(SelectedCell.Id));
        }
    }
}
