using System;
using System.Threading.Tasks;
using Xunit;
using CellManager.Models;
using CellManager.ViewModels;

namespace CellManager.Tests
{
    public class RunViewModelTests
    {
        [Fact]
        public void DemoSchedule_HasTimelineSteps()
        {
            var vm = new RunViewModel();
            Assert.NotNull(vm.SelectedSchedule);
            Assert.NotEmpty(vm.SelectedSchedule!.TestProfileIds);
        }

        [Fact]
        public async Task StartCommand_BeginsProgress()
        {
            var vm = new RunViewModel();
            var sched = new Schedule { Name = "Test", EstimatedDuration = TimeSpan.FromSeconds(1) };
            vm.AvailableSchedules.Add(sched);
            vm.SelectedSchedule = sched;
            vm.StartCommand.Execute(null);
            await Task.Delay(300);
            Assert.True(vm.Progress > 0);
        }

        [Fact]
        public async Task StopCommand_ResetsProgress()
        {
            var vm = new RunViewModel();
            var sched = new Schedule { Name = "Test", EstimatedDuration = TimeSpan.FromSeconds(1) };
            vm.AvailableSchedules.Add(sched);
            vm.SelectedSchedule = sched;
            vm.StartCommand.Execute(null);
            await Task.Delay(300);
            vm.StopCommand.Execute(null);
            Assert.Equal(0, vm.Progress);
            Assert.Equal(TimeSpan.Zero, vm.ElapsedTime);
        }
    }
}
