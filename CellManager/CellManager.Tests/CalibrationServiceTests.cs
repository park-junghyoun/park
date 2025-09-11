using CellManager.Services;
using Xunit;

namespace CellManager.Tests
{
    public class CalibrationServiceTests
    {
        [Fact]
        public void CalculateLinearCalibration_ComputesGainAndOffset()
        {
            var result = CalibrationService.CalculateLinearCalibration(1000, 950, 4000, 3900);
            Assert.NotNull(result);
            var (gain, offset) = result!.Value;
            Assert.Equal(1.016949, gain, 6);
            Assert.Equal(33.898305, offset, 6);
        }

        [Fact]
        public void CalculateLinearCalibration_ReturnsNullWhenMeasurementsEqual()
        {
            var result = CalibrationService.CalculateLinearCalibration(1000, 1000, 4000, 1000);
            Assert.Null(result);
        }
    }
}
