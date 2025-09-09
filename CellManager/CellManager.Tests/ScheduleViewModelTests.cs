using System.Collections.ObjectModel;
using System.Collections.Generic;
using Xunit;
using CellManager.ViewModels;
using CellManager.Models;
using CellManager.Models.TestProfile;
using CellManager.Services;

namespace CellManager.Tests
{
    public class ScheduleViewModelTests
    {
        private class DummyScheduleRepository : IScheduleRepository
        {
            public List<Schedule> GetAll() => new();
            public Schedule? GetById(int id) => null;
            public void Save(Schedule schedule) { }
            public void Delete(int id) { }
        }

        private class DummyChargeProfileRepository : IChargeProfileRepository
        {
            public ObservableCollection<ChargeProfile> Load(int cellId) => new();
            public void Save(ChargeProfile profile, int cellId) { }
            public void Delete(ChargeProfile profile) { }
        }

        private class DummyDischargeProfileRepository : IDischargeProfileRepository
        {
            public ObservableCollection<DischargeProfile> Load(int cellId) => new();
            public void Save(DischargeProfile profile, int cellId) { }
            public void Delete(DischargeProfile profile) { }
        }

        private class DummyEcmPulseProfileRepository : IEcmPulseProfileRepository
        {
            public ObservableCollection<ECMPulseProfile> Load(int cellId) => new();
            public void Save(ECMPulseProfile profile, int cellId) { }
            public void Delete(ECMPulseProfile profile) { }
        }

        private class DummyOcvProfileRepository : IOcvProfileRepository
        {
            public ObservableCollection<OCVProfile> Load(int cellId) => new();
            public void Save(OCVProfile profile, int cellId) { }
            public void Delete(OCVProfile profile) { }
        }

        private class DummyRestProfileRepository : IRestProfileRepository
        {
            public ObservableCollection<RestProfile> Load(int cellId) => new();
            public void Save(RestProfile profile, int cellId) { }
            public void Delete(RestProfile profile) { }
        }

        private static ScheduleViewModel CreateViewModel()
        {
            return new ScheduleViewModel(
                new DummyChargeProfileRepository(),
                new DummyDischargeProfileRepository(),
                new DummyEcmPulseProfileRepository(),
                new DummyOcvProfileRepository(),
                new DummyRestProfileRepository(),
                new DummyScheduleRepository());
        }

        [Fact]
        public void InsertProfile_AllowsDuplicateProfiles()
        {
            var vm = CreateViewModel();
            var profile = new ProfileReference { CellId = 1, Type = TestProfileType.Charge, Id = 1, Name = "P1" };
            vm.InsertProfile(profile, -1);
            vm.InsertProfile(profile, -1);
            Assert.Equal(2, vm.WorkingSchedule.Count);
        }

        [Fact]
        public void MoveProfile_ReordersExistingProfile()
        {
            var vm = CreateViewModel();
            var p1 = new ProfileReference { CellId = 1, Type = TestProfileType.Charge, Id = 1, Name = "P1" };
            var p2 = new ProfileReference { CellId = 1, Type = TestProfileType.Charge, Id = 2, Name = "P2" };
            vm.InsertProfile(p1, -1);
            vm.InsertProfile(p2, -1);
            var first = vm.WorkingSchedule[0];
            vm.MoveProfile(first, 2);
            Assert.Equal(p2, vm.WorkingSchedule[0].Reference);
            Assert.Equal(p1, vm.WorkingSchedule[1].Reference);
        }
    }
}

