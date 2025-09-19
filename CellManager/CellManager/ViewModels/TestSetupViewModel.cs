using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CellManager.Messages;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Services;
using CellManager.ViewModels.TestSetup;
using CellManager.Views.TestSetup;

namespace CellManager.ViewModels
{
    /// <summary>
    ///     Coordinates creation and maintenance of reusable test profiles for the selected cell.
    /// </summary>
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
        public RelayCommand<ChargeProfile> DuplicateChargeProfileCommand { get; }

        public RelayCommand AddDischargeProfileCommand { get; }
        public RelayCommand<DischargeProfile> EditDischargeProfileCommand { get; }
        public RelayCommand<DischargeProfile> DeleteDischargeProfileCommand { get; }
        public RelayCommand<DischargeProfile> DuplicateDischargeProfileCommand { get; }

        public RelayCommand AddRestProfileCommand { get; }
        public RelayCommand<RestProfile> EditRestProfileCommand { get; }
        public RelayCommand<RestProfile> DeleteRestProfileCommand { get; }
        public RelayCommand<RestProfile> DuplicateRestProfileCommand { get; }

        public RelayCommand AddOcvProfileCommand { get; }
        public RelayCommand<OCVProfile> EditOcvProfileCommand { get; }
        public RelayCommand<OCVProfile> DeleteOcvProfileCommand { get; }
        public RelayCommand<OCVProfile> DuplicateOcvProfileCommand { get; }

        public RelayCommand AddEcmProfileCommand { get; }
        public RelayCommand<ECMPulseProfile> EditEcmProfileCommand { get; }
        public RelayCommand<ECMPulseProfile> DeleteEcmProfileCommand { get; }
        public RelayCommand<ECMPulseProfile> DuplicateEcmProfileCommand { get; }

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
            DuplicateChargeProfileCommand = new RelayCommand<ChargeProfile>(DuplicateChargeProfile);

            AddDischargeProfileCommand = new RelayCommand(AddDischargeProfile, () => SelectedCell?.Id > 0);
            EditDischargeProfileCommand = new RelayCommand<DischargeProfile>(EditDischargeProfile);
            DeleteDischargeProfileCommand = new RelayCommand<DischargeProfile>(DeleteDischargeProfile);
            DuplicateDischargeProfileCommand = new RelayCommand<DischargeProfile>(DuplicateDischargeProfile);

            AddRestProfileCommand = new RelayCommand(AddRestProfile, () => SelectedCell?.Id > 0);
            EditRestProfileCommand = new RelayCommand<RestProfile>(EditRestProfile);
            DeleteRestProfileCommand = new RelayCommand<RestProfile>(DeleteRestProfile);
            DuplicateRestProfileCommand = new RelayCommand<RestProfile>(DuplicateRestProfile);

            AddOcvProfileCommand = new RelayCommand(AddOcvProfile, () => SelectedCell?.Id > 0);
            EditOcvProfileCommand = new RelayCommand<OCVProfile>(EditOcvProfile);
            DeleteOcvProfileCommand = new RelayCommand<OCVProfile>(DeleteOcvProfile);
            DuplicateOcvProfileCommand = new RelayCommand<OCVProfile>(DuplicateOcvProfile);

            AddEcmProfileCommand = new RelayCommand(AddEcmProfile, () => SelectedCell?.Id > 0);
            EditEcmProfileCommand = new RelayCommand<ECMPulseProfile>(EditEcmProfile);
            DeleteEcmProfileCommand = new RelayCommand<ECMPulseProfile>(DeleteEcmProfile);
            DuplicateEcmProfileCommand = new RelayCommand<ECMPulseProfile>(DuplicateEcmProfile);

            WeakReferenceMessenger.Default.Register<CellSelectedMessage>(this, (r, m) =>
            {
                SelectedCell = m.SelectedCell;
                ReloadAll();
                UpdateCanExecutes();
            });
        }

        partial void OnSelectedCellChanged(Cell value) => UpdateCanExecutes();

        /// <summary>Refreshes command availability whenever the selected cell changes.</summary>
        private void UpdateCanExecutes()
        {
            AddChargeProfileCommand.NotifyCanExecuteChanged();
            AddDischargeProfileCommand.NotifyCanExecuteChanged();
            AddRestProfileCommand.NotifyCanExecuteChanged();
            AddOcvProfileCommand.NotifyCanExecuteChanged();
            AddEcmProfileCommand.NotifyCanExecuteChanged();
        }

        /// <summary>Opens the editor for a new charge profile and persists it.</summary>
        private void AddChargeProfile()
        {
            var profile = new ChargeProfile { Name = "New Charge" };
            if (OpenEditor(profile))
            {
                _chargeRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        /// <summary>Creates an isolated copy of the profile for editing and saves changes.</summary>
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

        /// <summary>Deletes the supplied charge profile from the repository.</summary>
        private void DeleteChargeProfile(ChargeProfile profile)
        {
            if (profile == null) return;
            _chargeRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        /// <summary>Creates a copy of an existing charge profile and persists it as a new record.</summary>
        private void DuplicateChargeProfile(ChargeProfile profile)
        {
            if (profile == null || SelectedCell?.Id <= 0) return;

            var copy = Clone(profile);
            copy.Id = 0;
            copy.DisplayId = GetNextProfileId();
            copy.Name = GenerateCopyName(profile.Name, ChargeProfiles.Select(p => p.Name));

            _chargeRepo.Save(copy, SelectedCell.Id);
            ReloadAllAndNotify();
        }

        /// <summary>Creates a new discharge profile and saves it if the dialog is confirmed.</summary>
        private void AddDischargeProfile()
        {
            var profile = new DischargeProfile { Name = "New Discharge" };
            if (OpenEditor(profile))
            {
                _dischargeRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        /// <summary>Edits an existing discharge profile via a cloned instance.</summary>
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

        /// <summary>Removes the selected discharge profile.</summary>
        private void DeleteDischargeProfile(DischargeProfile profile)
        {
            if (profile == null) return;
            _dischargeRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        /// <summary>Duplicates a discharge profile and saves it as a new entity.</summary>
        private void DuplicateDischargeProfile(DischargeProfile profile)
        {
            if (profile == null || SelectedCell?.Id <= 0) return;

            var copy = Clone(profile);
            copy.Id = 0;
            copy.DisplayId = GetNextProfileId();
            copy.Name = GenerateCopyName(profile.Name, DischargeProfiles.Select(p => p.Name));

            _dischargeRepo.Save(copy, SelectedCell.Id);
            ReloadAllAndNotify();
        }

        /// <summary>Creates a rest profile template for the current cell.</summary>
        private void AddRestProfile()
        {
            var profile = new RestProfile { Name = "New Rest" };
            if (OpenEditor(profile))
            {
                _restRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        /// <summary>Edits an existing rest profile.</summary>
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

        /// <summary>Deletes a rest profile entry.</summary>
        private void DeleteRestProfile(RestProfile profile)
        {
            if (profile == null) return;
            _restRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        /// <summary>Creates a duplicated rest profile entry.</summary>
        private void DuplicateRestProfile(RestProfile profile)
        {
            if (profile == null || SelectedCell?.Id <= 0) return;

            var copy = Clone(profile);
            copy.Id = 0;
            copy.DisplayId = GetNextProfileId();
            copy.Name = GenerateCopyName(profile.Name, RestProfiles.Select(p => p.Name));

            _restRepo.Save(copy, SelectedCell.Id);
            ReloadAllAndNotify();
        }

        /// <summary>Initialises a new OCV profile and persists it.</summary>
        private void AddOcvProfile()
        {
            var profile = new OCVProfile { Name = "New OCV" };
            if (OpenEditor(profile))
            {
                _ocvRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        /// <summary>Opens the editor for an existing OCV profile.</summary>
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

        /// <summary>Removes the supplied OCV profile.</summary>
        private void DeleteOcvProfile(OCVProfile profile)
        {
            if (profile == null) return;
            _ocvRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        /// <summary>Copies an OCV profile and saves it under a new identifier.</summary>
        private void DuplicateOcvProfile(OCVProfile profile)
        {
            if (profile == null || SelectedCell?.Id <= 0) return;

            var copy = Clone(profile);
            copy.Id = 0;
            copy.DisplayId = GetNextProfileId();
            copy.Name = GenerateCopyName(profile.Name, OcvProfiles.Select(p => p.Name));

            _ocvRepo.Save(copy, SelectedCell.Id);
            ReloadAllAndNotify();
        }

        /// <summary>Creates an ECM pulse profile skeleton and saves it once confirmed.</summary>
        private void AddEcmProfile()
        {
            var profile = new ECMPulseProfile { Name = "New ECM" };
            if (OpenEditor(profile))
            {
                _ecmRepo.Save(profile, SelectedCell!.Id);
                ReloadAllAndNotify();
            }
        }

        /// <summary>Provides an editor for modifying an ECM pulse profile.</summary>
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

        /// <summary>Deletes an ECM pulse profile from storage.</summary>
        private void DeleteEcmProfile(ECMPulseProfile profile)
        {
            if (profile == null) return;
            _ecmRepo.Delete(profile);
            ReloadAllAndNotify();
        }

        /// <summary>Duplicates an ECM pulse profile and stores it for the selected cell.</summary>
        private void DuplicateEcmProfile(ECMPulseProfile profile)
        {
            if (profile == null || SelectedCell?.Id <= 0) return;

            var copy = Clone(profile);
            copy.Id = 0;
            copy.DisplayId = GetNextProfileId();
            copy.Name = GenerateCopyName(profile.Name, EcmPulseProfiles.Select(p => p.Name));

            _ecmRepo.Save(copy, SelectedCell.Id);
            ReloadAllAndNotify();
        }

        /// <summary>
        ///     Performs a deep clone using JSON serialization to avoid mutating the original objects.
        /// </summary>
        private static T Clone<T>(T source) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source))!;

        /// <summary>
        ///     Retrieves the next available profile identifier from the shared SQLite database.
        /// </summary>
        private int GetNextProfileId()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            var dbPath = Path.Combine(dataDir, "test_profiles.db");
            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();
            return ProfileIdProvider.GetNextId(conn);
        }

        /// <summary>
        ///     Applies display identifiers and opens the appropriate detail window for the supplied profile.
        /// </summary>
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

            TestProfileDetailViewModel viewModel = profile switch
            {
                ChargeProfile cp => new TestProfileDetailViewModel<ChargeProfile>(TestProfileType.Charge, cp),
                DischargeProfile dp => new TestProfileDetailViewModel<DischargeProfile>(TestProfileType.Discharge, dp),
                RestProfile rp => new TestProfileDetailViewModel<RestProfile>(TestProfileType.Rest, rp),
                OCVProfile op => new TestProfileDetailViewModel<OCVProfile>(TestProfileType.OCV, op),
                ECMPulseProfile ep => new TestProfileDetailViewModel<ECMPulseProfile>(TestProfileType.ECM, ep),
                _ => throw new ArgumentException("Unsupported profile type", nameof(profile))
            };

            var window = new ProfileDetailWindow { DataContext = viewModel };
            return window.ShowDialog() == true;
        }

        /// <summary>
        ///     Refreshes the in-memory collections for each profile category.
        /// </summary>
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

        /// <summary>Reloads profiles and informs listeners that the profile list has changed.</summary>
        private void ReloadAllAndNotify()
        {
            ReloadAll();
            NotifyProfilesUpdated();
        }

        /// <summary>Sends a messenger notification so schedules can update their references.</summary>
        private void NotifyProfilesUpdated()
        {
            if (SelectedCell?.Id > 0)
                WeakReferenceMessenger.Default.Send(new TestProfilesUpdatedMessage(SelectedCell.Id));
        }

        /// <summary>Produces a descriptive name for duplicated profiles while avoiding collisions.</summary>
        private static string GenerateCopyName(string? originalName, IEnumerable<string> existingNames)
        {
            var baseName = string.IsNullOrWhiteSpace(originalName) ? "Profile" : originalName.Trim();
            var candidate = $"{baseName} Copy";
            var suffix = 2;

            var existing = existingNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            while (existing.Contains(candidate))
            {
                candidate = $"{baseName} Copy {suffix++}";
            }

            return candidate;
        }
    }
}
