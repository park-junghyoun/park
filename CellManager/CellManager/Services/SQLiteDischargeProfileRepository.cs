using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    public class SQLiteDischargeProfileRepository : IDischargeProfileRepository
    {
        private readonly string _dbPath;
        public SQLiteDischargeProfileRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "test_profiles.db");
            Initialize();
        }
        private void Initialize()
        {
            if (!File.Exists(_dbPath)) SQLiteConnection.CreateFile(_dbPath);
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = @"
                CREATE TABLE IF NOT EXISTS DischargeProfiles(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CellId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    DischargeMode TEXT,
                    DischargeCurrent REAL,
                    DischargeCutoffVoltage REAL,
                    DischargeCapacityMah REAL
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
        public ObservableCollection<DischargeProfile> Load(int cellId)
        {
            var list = new ObservableCollection<DischargeProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, DischargeMode, DischargeCurrent, DischargeCutoffVoltage, DischargeCapacityMah FROM DischargeProfiles WHERE CellId=@CellId ORDER BY Name;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CellId", cellId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new DischargeProfile
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Name = Convert.ToString(r["Name"]),
                    DischargeMode = r["DischargeMode"] as string,
                    DischargeCurrent = r["DischargeCurrent"] == DBNull.Value ? 0 : Convert.ToDouble(r["DischargeCurrent"]),
                    DischargeCutoffVoltage = r["DischargeCutoffVoltage"] == DBNull.Value ? 0 : Convert.ToDouble(r["DischargeCutoffVoltage"]),
                    DischargeCapacityMah = r["DischargeCapacityMah"] == DBNull.Value ? 0 : Convert.ToDouble(r["DischargeCapacityMah"])
                });
            }
            return list;
        }
        public void Save(DischargeProfile p, int cellId)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            if (p.Id == 0)
            {
                var sql = @"INSERT INTO DischargeProfiles(CellId, Name, DischargeMode, DischargeCurrent, DischargeCutoffVoltage, DischargeCapacityMah)
                            VALUES (@CellId, @Name, @Mode, @Cur, @CutV, @Cap);
                            SELECT last_insert_rowid();";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New Discharge");
                cmd.Parameters.AddWithValue("@Mode", (object?)p.DischargeMode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cur", (object?)p.DischargeCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutV", (object?)p.DischargeCutoffVoltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cap", (object?)p.DischargeCapacityMah ?? DBNull.Value);
                p.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            else
            {
                var sql = @"UPDATE DischargeProfiles SET Name=@Name, DischargeMode=@Mode, DischargeCurrent=@Cur,
                            DischargeCutoffVoltage=@CutV, DischargeCapacityMah=@Cap WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "Discharge");
                cmd.Parameters.AddWithValue("@Mode", (object?)p.DischargeMode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cur", (object?)p.DischargeCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutV", (object?)p.DischargeCutoffVoltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cap", (object?)p.DischargeCapacityMah ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        public void Delete(DischargeProfile p)
        {
            if (p?.Id <= 0) return;
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM DischargeProfiles WHERE Id=@Id;", conn);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
