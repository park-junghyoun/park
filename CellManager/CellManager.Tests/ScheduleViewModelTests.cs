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
        public void DeleteScheduleCommand_RemovesSchedule()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var target = vm.SelectedSchedule;
            vm.DeleteScheduleCommand.Execute(vm.SelectedSchedule);
            Assert.DoesNotContain(target, vm.Schedules);
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
        public void InsertStep_IgnoresAdditionalLoopMarkers()
        {
            var vm = new ScheduleViewModel();
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);

            vm.InsertStep(start, -1);
            vm.InsertStep(start, -1);
            vm.InsertStep(end, -1);
            vm.InsertStep(end, -1);

            Assert.Equal(1, vm.Sequence.Count(s => s.Kind == StepKind.LoopStart));
            Assert.Equal(1, vm.Sequence.Count(s => s.Kind == StepKind.LoopEnd));
        }

        [Fact]
        public void SaveScheduleCommand_ClearsLoopIndices_WhenEndMissing()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            vm.InsertStep(start, -1);

            Assert.True(vm.LoopStartIndex > 0);
            Assert.True(vm.SaveScheduleCommand.CanExecute(null));

            vm.SaveScheduleCommand.Execute(null);

            Assert.Equal(0, vm.LoopStartIndex);
            Assert.Equal(0, vm.LoopEndIndex);
            Assert.Equal(0, vm.SelectedSchedule?.LoopStartIndex);
            Assert.Equal(0, vm.SelectedSchedule?.LoopEndIndex);
        }

        [Fact]
        public void SaveScheduleCommand_ClearsLoopIndices_WhenStartMissing()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);
            vm.InsertStep(end, -1);

            Assert.True(vm.LoopEndIndex > 0);
            Assert.True(vm.SaveScheduleCommand.CanExecute(null));

            vm.SaveScheduleCommand.Execute(null);

            Assert.Equal(0, vm.LoopStartIndex);
            Assert.Equal(0, vm.LoopEndIndex);
            Assert.Equal(0, vm.SelectedSchedule?.LoopStartIndex);
            Assert.Equal(0, vm.SelectedSchedule?.LoopEndIndex);
        }

        [Fact]
        public void SaveScheduleCommand_Disabled_WhenEndPrecedesStart()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);

            vm.InsertStep(end, -1);
            vm.InsertStep(start, -1);

            Assert.False(vm.SaveScheduleCommand.CanExecute(null));

            vm.SaveScheduleCommand.Execute(null);

            Assert.Equal(0, vm.SelectedSchedule?.LoopStartIndex);
            Assert.Equal(0, vm.SelectedSchedule?.LoopEndIndex);
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
        public void TotalDuration_IncludesLoopRepeatCount()
        {
            var vm = new ScheduleViewModel();
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);
            var template = vm.StepLibrary.First(g => g.Name != "Loop").Steps.First();

            vm.InsertStep(start, -1);
            vm.InsertStep(template, -1);
            vm.InsertStep(end, -1);

            vm.RepeatCount = 3;

            Assert.Equal(TimeSpan.FromHours(3), vm.TotalDuration);
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

        [Fact]
        public void SelectedSchedule_TracksEstimatedDuration_WithLoopRepeatCount()
        {
            var vm = new ScheduleViewModel();
            vm.AddScheduleCommand.Execute(null);
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);
            var template = vm.StepLibrary.First(g => g.Name != "Loop").Steps.First();

            vm.InsertStep(start, -1);
            vm.InsertStep(template, -1);
            vm.InsertStep(end, -1);

            vm.RepeatCount = 4;

            Assert.Equal(TimeSpan.FromHours(4), vm.SelectedSchedule?.EstimatedDuration);
        }

        [Fact]
        public void CalendarDays_ReflectSequenceOrder()
        {
            var vm = new ScheduleViewModel();
            var template = vm.StepLibrary.First().Steps.First();
            vm.InsertStep(template, -1);
            vm.InsertStep(template, -1);

            var entries = vm.CalendarDays.SelectMany(day => day.Entries).ToList();

            Assert.Equal(2, entries.Count);
            Assert.Equal(new[] { 1, 2 }, entries.Select(e => e.Order));
            Assert.All(entries, e => Assert.False(e.IsLoopSegment));
        }

        [Fact]
        public void CalendarDays_RepeatLoopSegmentPerIteration()
        {
            var vm = new ScheduleViewModel();
            var loopGroup = vm.StepLibrary.First(g => g.Name == "Loop");
            var start = loopGroup.Steps.First(s => s.Kind == StepKind.LoopStart);
            var end = loopGroup.Steps.First(s => s.Kind == StepKind.LoopEnd);
            var profile = vm.StepLibrary.First(g => g.Name != "Loop").Steps.First();

            vm.InsertStep(start, -1);
            vm.InsertStep(profile, -1);
            vm.InsertStep(end, -1);
            vm.RepeatCount = 3;

            var loopEntries = vm.CalendarDays.SelectMany(day => day.Entries).ToList();

            Assert.Equal(3, loopEntries.Count);
            Assert.All(loopEntries, e => Assert.True(e.IsLoopSegment));
            Assert.Equal(new[] { 1, 2, 3 }, loopEntries.Select(e => e.LoopIteration));
        }
    }
}
