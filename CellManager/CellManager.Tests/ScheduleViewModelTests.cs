using System;
using System.Linq;
using Xunit;
using CellManager.ViewModels;

namespace CellManager.Tests
{
    public class ScheduleViewModelTests
    {
        [Fact]
        public void InsertStep_AddsCloneToSequence()
        {
            var vm = new ScheduleViewModel();
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            Assert.Single(vm.Sequence);
            Assert.NotSame(template, vm.Sequence.First());
        }

        [Fact]
        public void MoveStep_ReordersSequence()
        {
            var vm = new ScheduleViewModel();
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            vm.InsertStep(template, -1);
            var first = vm.Sequence.First();
            vm.MoveStep(first, 2);
            Assert.NotEqual(first, vm.Sequence.First());
        }

        [Fact]
        public void AddScheduleCommand_CreatesNewSchedule()
        {
            var vm = new ScheduleViewModel();
            var initialCount = vm.Schedules.Count;
            vm.AddScheduleCommand.Execute(null);
            Assert.Equal(initialCount + 1, vm.Schedules.Count);
            Assert.Equal(vm.SelectedSchedule, vm.Schedules.Last());
            Assert.Equal(initialCount + 1, vm.SelectedSchedule?.Ordering);
        }

        [Fact]
        public void SaveScheduleCommand_PersistsSequence()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            vm.ScheduleName = "My Schedule";
            vm.SaveScheduleCommand.Execute(null);
            Assert.Equal("My Schedule", vm.SelectedSchedule?.Name);
            Assert.Contains(template.Id, vm.SelectedSchedule?.TestProfileIds);
        }

        [Fact]
        public void LoopMarkers_UpdateIndices()
        {
            var vm = new ScheduleViewModel();
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);
            vm.InsertStep(start, -1);
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            vm.InsertStep(end, -1);
            Assert.Equal(1, vm.LoopStartIndex);
            Assert.Equal(3, vm.LoopEndIndex);
        }

        [Fact]
        public void TotalDuration_SumsStepDurations()
        {
            var vm = new ScheduleViewModel();
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            vm.InsertStep(template, -1);
            Assert.Equal(TimeSpan.FromHours(2), vm.TotalDuration);
        }

        [Fact]
        public void SelectedSchedule_TracksEstimatedDuration()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            vm.InsertStep(template, -1);
            Assert.Equal(TimeSpan.FromHours(2), vm.SelectedSchedule?.EstimatedDuration);
        }
    }
}
