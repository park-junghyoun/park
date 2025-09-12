using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Xunit;
using CellManager.Models;
using CellManager.Services;
using CellManager.ViewModels;
using CellManager.Messages;

namespace CellManager.Tests
{
    public class RunViewModelTests
    {
        private class FakeScheduleRepository : IScheduleRepository
        {
            public List<Schedule> Load(int cellId) => new()
            {
                new Schedule { Id = 1, CellId = cellId, Name = "Sched A" },
                new Schedule { Id = 2, CellId = cellId, Name = "Sched B" }
            };

            public int Save(int cellId, Schedule schedule) => schedule.Id;
            public void Delete(int cellId, int id) { }
        }

        [Fact]
        public void LoadsSchedules_OnCellSelected()
        {
            WeakReferenceMessenger.Default.Reset();
            var repo = new FakeScheduleRepository();
            var vm = new RunViewModel(repo);
            var cell = new Cell { Id = 5 };
            WeakReferenceMessenger.Default.Send(new CellSelectedMessage(cell));
            Assert.Equal(2, vm.AvailableSchedules.Count);
            Assert.Equal(vm.AvailableSchedules[0], vm.SelectedSchedule);
        }
    }
}
