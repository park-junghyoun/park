using System;
using System.Collections.Generic;
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
                    CutoffCurrent REAL,
                    ChargeMode INTEGER,
                    ChargeCapacityMah REAL,
                    ChargeTimeSeconds INTEGER
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            var existing = new HashSet<string>();
            using (var infoCmd = new SQLiteCommand("PRAGMA table_info(ChargeProfiles);", conn))
            using (var reader = infoCmd.ExecuteReader())
                while (reader.Read()) existing.Add(Convert.ToString(reader["name"]));

            if (!existing.Contains("ChargeMode"))
            {
                new SQLiteCommand("ALTER TABLE ChargeProfiles ADD COLUMN ChargeMode INTEGER;", conn).ExecuteNonQuery();
                new SQLiteCommand("UPDATE ChargeProfiles SET ChargeMode = 2;", conn).ExecuteNonQuery();
            }
            if (!existing.Contains("ChargeCapacityMah"))
                new SQLiteCommand("ALTER TABLE ChargeProfiles ADD COLUMN ChargeCapacityMah REAL;", conn).ExecuteNonQuery();
            if (!existing.Contains("ChargeTimeSeconds"))
                new SQLiteCommand("ALTER TABLE ChargeProfiles ADD COLUMN ChargeTimeSeconds INTEGER;", conn).ExecuteNonQuery();
        }

        public ObservableCollection<ChargeProfile> Load(int cellId)
        {
            var list = new ObservableCollection<ChargeProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, ChargeCurrent, ChargeCutoffVoltage, CutoffCurrent, ChargeMode, ChargeCapacityMah, ChargeTimeSeconds FROM ChargeProfiles WHERE CellId=@CellId ORDER BY Name;";
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
                    ChargeMode = r["ChargeMode"] == DBNull.Value ? ChargeMode.FullCharge : (ChargeMode)Convert.ToInt32(r["ChargeMode"]),
                    ChargeCapacityMah = r["ChargeCapacityMah"] == DBNull.Value ? null : Convert.ToDouble(r["ChargeCapacityMah"]),
                    ChargeTime = r["ChargeTimeSeconds"] == DBNull.Value ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToInt32(r["ChargeTimeSeconds"])),
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
                var sql = @"INSERT INTO ChargeProfiles(CellId, Name, ChargeCurrent, ChargeCutoffVoltage, CutoffCurrent, ChargeMode, ChargeCapacityMah, ChargeTimeSeconds)
                            VALUES (@CellId, @Name, @ChargeCurrent, @ChargeCutoffVoltage, @CutoffCurrent, @ChargeMode, @ChargeCapacityMah, @ChargeTimeSeconds);
                            SELECT last_insert_rowid();";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New Charge");
                cmd.Parameters.AddWithValue("@ChargeCurrent", (object?)p.ChargeCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeCutoffVoltage", (object?)p.ChargeCutoffVoltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutoffCurrent", (object?)p.CutoffCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeMode", (int)p.ChargeMode);
                cmd.Parameters.AddWithValue("@ChargeCapacityMah", (object?)p.ChargeCapacityMah ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeTimeSeconds", (int)p.ChargeTime.TotalSeconds);
                p.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            else
            {
                var sql = @"UPDATE ChargeProfiles SET Name=@Name, ChargeCurrent=@ChargeCurrent,
                            ChargeCutoffVoltage=@ChargeCutoffVoltage, CutoffCurrent=@CutoffCurrent,
                            ChargeMode=@ChargeMode, ChargeCapacityMah=@ChargeCapacityMah, ChargeTimeSeconds=@ChargeTimeSeconds
                            WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "Charge");
                cmd.Parameters.AddWithValue("@ChargeCurrent", (object?)p.ChargeCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeCutoffVoltage", (object?)p.ChargeCutoffVoltage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutoffCurrent", (object?)p.CutoffCurrent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeMode", (int)p.ChargeMode);
                cmd.Parameters.AddWithValue("@ChargeCapacityMah", (object?)p.ChargeCapacityMah ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChargeTimeSeconds", (int)p.ChargeTime.TotalSeconds);
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
