using System;

namespace CellManager.Services
{
    /// <summary>
    /// Provides helper methods for calculating linear calibration values
    /// from reference and measured readings.
    /// </summary>
    public static class CalibrationService
    {
        /// <summary>
        /// Calculates a linear gain and offset given two reference points.
        /// </summary>
        /// <param name="referenceLow">The reference value for the low reading.</param>
        /// <param name="actualLow">The actual measured value for the low reading.</param>
        /// <param name="referenceHigh">The reference value for the high reading.</param>
        /// <param name="actualHigh">The actual measured value for the high reading.</param>
        /// <returns>
        /// A tuple containing gain and offset that maps measured values to reference values,
        /// or <c>null</c> if the calculation cannot be performed.
        /// </returns>
        public static (double Gain, double Offset)? CalculateLinearCalibration(
            double referenceLow,
            double actualLow,
            double referenceHigh,
            double actualHigh)
        {
            var denominator = actualHigh - actualLow;
            if (Math.Abs(denominator) < double.Epsilon)
            {
                return null;
            }

            var gain = (referenceHigh - referenceLow) / denominator;
            var offset = referenceLow - gain * actualLow;
            return (gain, offset);
        }
    }
}
