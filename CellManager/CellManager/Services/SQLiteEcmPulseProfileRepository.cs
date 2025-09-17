using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>SQLite persistence for ECM pulse profile definitions.</summary>
    public class SQLiteEcmPulseProfileRepository : IEcmPulseProfileRepository
    {
        private readonly string _dbPath;
        public SQLiteEcmPulseProfileRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "test_profiles.db");
            Initialize();
        }
        /// <summary>Creates the ECM pulse profile table if it is absent.</summary>
        private void Initialize()
        {
            if (!File.Exists(_dbPath)) SQLiteConnection.CreateFile(_dbPath);
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = @"
                CREATE TABLE IF NOT EXISTS EcmPulseProfiles(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CellId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    PulseCurrent REAL,
                    PulseDuration REAL,
                    ResetTimeAfterPulse REAL,
                    SamplingRateMs REAL
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
        /// <inheritdoc />
        public ObservableCollection<ECMPulseProfile> Load(int cellId)
        {
            var list = new ObservableCollection<ECMPulseProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, PulseCurrent, PulseDuration, ResetTimeAfterPulse, SamplingRateMs FROM EcmPulseProfiles WHERE CellId=@CellId ORDER BY Name;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CellId", cellId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ECMPulseProfile
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Name = Convert.ToString(r["Name"]),
                    PulseCurrent = r["PulseCurrent"] == DBNull.Value ? 0 : Convert.ToDouble(r["PulseCurrent"]),
                    PulseDuration = r["PulseDuration"] == DBNull.Value ? 0 : Convert.ToDouble(r["PulseDuration"]),
                    ResetTimeAfterPulse = r["ResetTimeAfterPulse"] == DBNull.Value ? 0 : Convert.ToDouble(r["ResetTimeAfterPulse"]),
                    SamplingRateMs = r["SamplingRateMs"] == DBNull.Value ? 0 : Convert.ToDouble(r["SamplingRateMs"])
                });
            }
            return list;
        }
        /// <inheritdoc />
        public void Save(ECMPulseProfile p, int cellId)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            if (p.Id == 0)
            {
                p.Id = ProfileIdProvider.GetNextId(conn);
                var sql = @"INSERT INTO EcmPulseProfiles(Id, CellId, Name, PulseCurrent, PulseDuration, ResetTimeAfterPulse, SamplingRateMs)
                            VALUES (@Id, @CellId, @Name, @Cur, @Dur, @Reset, @Rate);";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New ECM");
                cmd.Parameters.AddWithValue("@Cur", (object?)p.PulseCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Dur", (object?)p.PulseDuration ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reset", (object?)p.ResetTimeAfterPulse ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Rate", (object?)p.SamplingRateMs ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE EcmPulseProfiles SET Name=@Name, PulseCurrent=@Cur, PulseDuration=@Dur,
                            ResetTimeAfterPulse=@Reset, SamplingRateMs=@Rate WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "ECM");
                cmd.Parameters.AddWithValue("@Cur", (object?)p.PulseCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Dur", (object?)p.PulseDuration ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reset", (object?)p.ResetTimeAfterPulse ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Rate", (object?)p.SamplingRateMs ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        /// <inheritdoc />
        public void Delete(ECMPulseProfile p)
        {
            if (p?.Id <= 0) return;
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM EcmPulseProfiles WHERE Id=@Id;", conn);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
