using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    public class SQLiteChargeProfileRepository : IChargeProfileRepository
    {
        private readonly string _dbPath;

        public SQLiteChargeProfileRepository()
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
                CREATE TABLE IF NOT EXISTS ChargeProfiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CellId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    ChargeCurrent REAL,
                    ChargeCutoffVoltage REAL,
                    CutoffCurrent REAL
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public ObservableCollection<ChargeProfile> Load(int cellId)
        {
            var list = new ObservableCollection<ChargeProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, ChargeCurrent, ChargeCutoffVoltage, CutoffCurrent FROM ChargeProfiles WHERE CellId=@CellId ORDER BY Name;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CellId", cellId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ChargeProfile
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Name = Convert.ToString(r["Name"]),
                    ChargeCurrent = r["ChargeCurrent"] == DBNull.Value ? 0 : Convert.ToDouble(r["ChargeCurrent"]),
                    ChargeCutoffVoltage = r["ChargeCutoffVoltage"] == DBNull.Value ? 0 : Convert.ToDouble(r["ChargeCutoffVoltage"]),
                    CutoffCurrent = r["CutoffCurrent"] == DBNull.Value ? 0 : Convert.ToDouble(r["CutoffCurrent"]),
                });
            }
            return list;
        }

        public void Save(ChargeProfile p, int cellId)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            if (p.Id == 0)
            {
                var sql = @"INSERT INTO ChargeProfiles(CellId, Name, ChargeCurrent, ChargeCutoffVoltage, CutoffCurrent)
                            VALUES (@CellId, @Name, @ChargeCurrent, @ChargeCutoffVoltage, @CutoffCurrent);
                            SELECT last_insert_rowid();";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New Charge");
                cmd.Parameters.AddWithValue("@ChargeCurrent", (object?)p.ChargeCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeCutoffVoltage", (object?)p.ChargeCutoffVoltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutoffCurrent", (object?)p.CutoffCurrent ?? DBNull.Value);
                p.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            else
            {
                var sql = @"UPDATE ChargeProfiles SET Name=@Name, ChargeCurrent=@ChargeCurrent,
                            ChargeCutoffVoltage=@ChargeCutoffVoltage, CutoffCurrent=@CutoffCurrent
                            WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "Charge");
                cmd.Parameters.AddWithValue("@ChargeCurrent", (object?)p.ChargeCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeCutoffVoltage", (object?)p.ChargeCutoffVoltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutoffCurrent", (object?)p.CutoffCurrent ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(ChargeProfile p)
        {
            if (p?.Id <= 0) return;
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM ChargeProfiles WHERE Id=@Id;", conn);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
