using CellManager.Models;
using CellManager.Models.TestProfile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
namespace CellManager.Services
{
    public class SQLiteTestProfileRepository : ITestProfileRepository
    {
        private const string TestProfileDbFileName = "test_profiles.db";
        private readonly string _dbPath;

        public SQLiteTestProfileRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, TestProfileDbFileName);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        string createTestProfilesTableSql = @"
                        CREATE TABLE IF NOT EXISTS TestProfiles (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProfileName TEXT NOT NULL,
                            CellId INTEGER,
                            ChargeCurrent REAL,
                            ChargeCutoffVoltage REAL,
                            CutoffCurrent_Charge REAL,
                            DischargeMode TEXT,
                            DischargeCurrent REAL,
                            DischargeCutoffVoltage REAL,
                            DischargeCapacityMah REAL,
                            RestTime REAL,
                            Qmax REAL,
                            SocStepPercent REAL,
                            DischargeCurrent_OCV REAL,
                            RestTime_OCV REAL,
                            DischargeCutoffVoltage_OCV REAL,
                            PulseCurrent REAL,
                            PulseDuration REAL,
                            ResetTimeAfterPulse REAL,
                            SamplingRateMs REAL
                        );";
                        using (var cmd = new SQLiteCommand(createTestProfilesTableSql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test Profile Database initialization failed: {ex.Message}");
            }
        }

        public void SaveTestProfile(TestProfileModel profile)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();

                    if (profile.Id == 0)
                    {
                        const string insertSql = @"
                            INSERT INTO TestProfiles (
                                ProfileName, CellId,
                                ChargeCurrent, ChargeCutoffVoltage, CutoffCurrent_Charge,
                                DischargeMode, DischargeCurrent, DischargeCutoffVoltage, DischargeCapacityMah,
                                RestTime,
                                Qmax, SocStepPercent, DischargeCurrent_OCV, RestTime_OCV, DischargeCutoffVoltage_OCV,
                                PulseCurrent, PulseDuration, ResetTimeAfterPulse, SamplingRateMs
                            ) VALUES (
                                @ProfileName, @CellId,
                                @ChargeCurrent, @ChargeCutoffVoltage, @CutoffCurrent_Charge,
                                @DischargeMode, @DischargeCurrent, @DischargeCutoffVoltage, @DischargeCapacityMah,
                                @RestTime,
                                @Qmax, @SocStepPercent, @DischargeCurrent_OCV, @RestTime_OCV, @DischargeCutoffVoltage_OCV,
                                @PulseCurrent, @PulseDuration, @ResetTimeAfterPulse, @SamplingRateMs
                            );
                        ";
                        using (var cmd = new SQLiteCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
                            cmd.Parameters.AddWithValue("@CellId", profile.CellId);
                            cmd.Parameters.AddWithValue("@ChargeCurrent", profile.ChargeProfile?.ChargeCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ChargeCutoffVoltage", profile.ChargeProfile?.ChargeCutoffVoltage ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CutoffCurrent_Charge", profile.ChargeProfile?.CutoffCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeMode", profile.DischargeProfile?.DischargeMode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCurrent", profile.DischargeProfile?.DischargeCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCutoffVoltage", profile.DischargeProfile?.DischargeCutoffVoltage ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCapacityMah", profile.DischargeProfile?.DischargeCapacityMah ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@RestTime", profile.RestProfile?.RestTime ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Qmax", profile.OcvProfile?.Qmax ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@SocStepPercent", profile.OcvProfile?.SocStepPercent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCurrent_OCV", profile.OcvProfile?.DischargeCurrent_OCV ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@RestTime_OCV", profile.OcvProfile?.RestTime_OCV ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCutoffVoltage_OCV", profile.OcvProfile?.DischargeCutoffVoltage_OCV ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PulseCurrent", profile.EcmPulseProfile?.PulseCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PulseDuration", profile.EcmPulseProfile?.PulseDuration ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ResetTimeAfterPulse", profile.EcmPulseProfile?.ResetTimeAfterPulse ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@SamplingRateMs", profile.EcmPulseProfile?.SamplingRateMs ?? (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                            profile.Id = (int)conn.LastInsertRowId;
                        }
                    }
                    else
                    {
                        string updateSql = @"
                            UPDATE TestProfiles SET
                                ProfileName = @ProfileName, CellId = @CellId, ChargeCurrent = @ChargeCurrent,
                                ChargeCutoffVoltage = @ChargeCutoffVoltage, CutoffCurrent_Charge = @CutoffCurrent_Charge,
                                DischargeMode = @DischargeMode, DischargeCurrent = @DischargeCurrent,
                                DischargeCutoffVoltage = @DischargeCutoffVoltage, DischargeCapacityMah = @DischargeCapacityMah,
                                RestTime = @RestTime, Qmax = @Qmax, SocStepPercent = @SocStepPercent,
                                DischargeCurrent_OCV = @DischargeCurrent_OCV, RestTime_OCV = @RestTime_OCV,
                                DischargeCutoffVoltage_OCV = @DischargeCutoffVoltage_OCV, PulseCurrent = @PulseCurrent,
                                PulseDuration = @PulseDuration, ResetTimeAfterPulse = @ResetTimeAfterPulse,
                                SamplingRateMs = @SamplingRateMs
                            WHERE Id = @Id
                        ";
                        using (var cmd = new SQLiteCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", profile.Id);
                            cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
                            cmd.Parameters.AddWithValue("@CellId", profile.CellId);
                            cmd.Parameters.AddWithValue("@ChargeCurrent", profile.ChargeProfile?.ChargeCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ChargeCutoffVoltage", profile.ChargeProfile?.ChargeCutoffVoltage ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CutoffCurrent_Charge", profile.ChargeProfile?.CutoffCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeMode", profile.DischargeProfile?.DischargeMode ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCurrent", profile.DischargeProfile?.DischargeCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCutoffVoltage", profile.DischargeProfile?.DischargeCutoffVoltage ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCapacityMah", profile.DischargeProfile?.DischargeCapacityMah ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@RestTime", profile.RestProfile?.RestTime ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Qmax", profile.OcvProfile?.Qmax ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@SocStepPercent", profile.OcvProfile?.SocStepPercent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCurrent_OCV", profile.OcvProfile?.DischargeCurrent_OCV ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@RestTime_OCV", profile.OcvProfile?.RestTime_OCV ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DischargeCutoffVoltage_OCV", profile.OcvProfile?.DischargeCutoffVoltage_OCV ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PulseCurrent", profile.EcmPulseProfile?.PulseCurrent ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PulseDuration", profile.EcmPulseProfile?.PulseDuration ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ResetTimeAfterPulse", profile.EcmPulseProfile?.ResetTimeAfterPulse ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@SamplingRateMs", profile.EcmPulseProfile?.SamplingRateMs ?? (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save Test Profile failed: {ex.Message}");
            }
        }

        public void DeleteTestProfile(TestProfileModel profile)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();
                    string sql = "DELETE FROM TestProfiles WHERE Id = @Id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", profile.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete Test Profile failed: {ex.Message}");
            }
        }

        public ObservableCollection<TestProfileModel> LoadTestProfiles(int cellId)
        {
            var profiles = new ObservableCollection<TestProfileModel>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();
                    string sql = "SELECT * FROM TestProfiles WHERE CellId = @CellId";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CellId", cellId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var profile = new TestProfileModel
                                {
                                    Id = reader.GetInt32(0),
                                    ProfileName = reader.GetString(1),
                                    CellId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                    ChargeProfile = new ChargeProfile
                                    {
                                        ChargeCurrent = reader.IsDBNull(3) ? 0.0 : reader.GetDouble(3),
                                        ChargeCutoffVoltage = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4),
                                        CutoffCurrent = reader.IsDBNull(5) ? 0.0 : reader.GetDouble(5)
                                    },
                                    DischargeProfile = new DischargeProfile
                                    {
                                        DischargeMode = reader.IsDBNull(6) ? null : reader.GetString(6),
                                        DischargeCurrent = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7),
                                        DischargeCutoffVoltage = reader.IsDBNull(8) ? 0.0 : reader.GetDouble(8),
                                        DischargeCapacityMah = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9)
                                    },
                                    RestProfile = new RestProfile
                                    {
                                        RestTime = reader.IsDBNull(10) ? 0.0 : reader.GetDouble(10)
                                    },
                                    OcvProfile = new OCVProfile
                                    {
                                        Qmax = reader.IsDBNull(11) ? 0.0 : reader.GetDouble(11),
                                        SocStepPercent = reader.IsDBNull(12) ? 0.0 : reader.GetDouble(12),
                                        DischargeCurrent_OCV = reader.IsDBNull(13) ? 0.0 : reader.GetDouble(13),
                                        RestTime_OCV = reader.IsDBNull(14) ? 0.0 : reader.GetDouble(14),
                                        DischargeCutoffVoltage_OCV = reader.IsDBNull(15) ? 0.0 : reader.GetDouble(15)
                                    },
                                    EcmPulseProfile = new ECMPulseProfile
                                    {
                                        PulseCurrent = reader.IsDBNull(16) ? 0.0 : reader.GetDouble(16),
                                        PulseDuration = reader.IsDBNull(17) ? 0.0 : reader.GetDouble(17),
                                        ResetTimeAfterPulse = reader.IsDBNull(18) ? 0.0 : reader.GetDouble(18),
                                        SamplingRateMs = reader.IsDBNull(19) ? 0.0 : reader.GetDouble(19)
                                    }
                                };
                                profiles.Add(profile);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load Test Profiles failed: {ex.Message}");
            }
            return profiles;
        }
    }
}