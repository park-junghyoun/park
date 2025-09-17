using System;
using CellManager.Models.TestProfile;

namespace CellManager.Models
{
    /// <summary>
    ///     Provides heuristics for estimating how long different profile steps will take for a given cell.
    /// </summary>
    public static class ScheduleTimeCalculator
    {
        /// <summary>
        ///     Returns an estimated duration for the supplied profile instance, guarding against invalid data.
        /// </summary>
        public static TimeSpan? EstimateDuration(Cell cell, TestProfileType type, object? profile)
        {
            try
            {
                if (cell == null)
                    throw new ArgumentNullException(nameof(cell));
                return type switch
                {
                    TestProfileType.ECM => TimeSpan.FromMinutes(30),
                    TestProfileType.Rest => EstimateRest(profile as RestProfile),
                    TestProfileType.OCV => EstimateOcv(profile as OCVProfile),
                    TestProfileType.Charge => EstimateCharge(cell, profile as ChargeProfile),
                    TestProfileType.Discharge => EstimateDischarge(cell, profile as DischargeProfile),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Validates and returns the rest duration contained in the profile.</summary>
        private static TimeSpan? EstimateRest(RestProfile? profile)
        {
            if (profile == null || profile.RestTime < TimeSpan.Zero)
                throw new ArgumentException("Invalid rest profile");
            return profile.RestTime;
        }

        /// <summary>Estimates total time for an OCV sweep including per-step rest periods.</summary>
        private static TimeSpan? EstimateOcv(OCVProfile? profile)
        {
            if (profile == null || profile.DischargeCurrent_OCV <= 0 || profile.SocStepPercent <= 0)
                throw new ArgumentException("Invalid OCV profile");
            var dischargeSeconds = (profile.Qmax / profile.DischargeCurrent_OCV) * 3600.0;
            var restSeconds = (100.0 / profile.SocStepPercent) * profile.RestTime_OCV;
            return TimeSpan.FromSeconds(dischargeSeconds + restSeconds);
        }

        /// <summary>Determines charge duration based on the configured termination mode.</summary>
        private static TimeSpan? EstimateCharge(Cell cell, ChargeProfile? profile)
        {
            if (profile == null || profile.ChargeCurrent <= 0)
                throw new ArgumentException("Invalid charge profile");
            if (profile.ChargeMode == ChargeMode.ChargeByTime)
                return profile.ChargeTime;
            if (profile.ChargeMode == ChargeMode.ChargeByCapacity)
            {
                if (profile.ChargeCapacityMah == null)
                    throw new ArgumentException("Charge capacity missing");
                return TimeSpan.FromHours(profile.ChargeCapacityMah.Value / profile.ChargeCurrent);
            }
            if (profile.ChargeMode == ChargeMode.FullCharge)
            {
                if (cell.RatedCapacity <= 0 || cell.CutOffCurrent_Charge < 0 || profile.ChargeCurrent <= cell.CutOffCurrent_Charge)
                    throw new ArgumentException("Invalid cell or current values");
                return TimeSpan.FromHours((cell.RatedCapacity / profile.ChargeCurrent) * 1.4);
            }
            return null;
        }

        /// <summary>Calculates discharge time according to the selected cutoff condition.</summary>
        private static TimeSpan? EstimateDischarge(Cell cell, DischargeProfile? profile)
        {
            if (profile == null || profile.DischargeCurrent <= 0)
                throw new ArgumentException("Invalid discharge profile");
            return profile.DischargeMode switch
            {
                DischargeMode.DischargeByTime => profile.DischargeTime,
                DischargeMode.DischargeByCapacity => profile.DischargeCapacityMah.HasValue
                    ? TimeSpan.FromHours(profile.DischargeCapacityMah.Value / profile.DischargeCurrent)
                    : throw new ArgumentException("Discharge capacity missing"),
                DischargeMode.FullDischarge => cell.RatedCapacity > 0
                    ? TimeSpan.FromHours(cell.RatedCapacity / profile.DischargeCurrent)
                    : throw new ArgumentException("Invalid cell capacity"),
                _ => null
            };
        }
    }
}
