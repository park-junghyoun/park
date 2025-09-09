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
    }
}
