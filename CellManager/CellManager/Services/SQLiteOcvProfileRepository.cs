using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>SQLite-backed storage for OCV profiles.</summary>
    public class SQLiteOcvProfileRepository : IOcvProfileRepository
    {
        private readonly string _dbPath;
        public SQLiteOcvProfileRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "test_profiles.db");
            Initialize();
        }
        /// <summary>Creates the OCV profile table when it is missing.</summary>
        private void Initialize()
        {
            if (!File.Exists(_dbPath)) SQLiteConnection.CreateFile(_dbPath);
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = @"
                CREATE TABLE IF NOT EXISTS OcvProfiles(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CellId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Qmax REAL,
                    SocStepPercent REAL,
                    DischargeCurrent_OCV REAL,
                    RestTime_OCV REAL,
                    DischargeCutoffVoltage_OCV REAL
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
        /// <inheritdoc />
        public ObservableCollection<OCVProfile> Load(int cellId)
        {
            var list = new ObservableCollection<OCVProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, Qmax, SocStepPercent, DischargeCurrent_OCV, RestTime_OCV, DischargeCutoffVoltage_OCV FROM OcvProfiles WHERE CellId=@CellId ORDER BY Name;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CellId", cellId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new OCVProfile
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Name = Convert.ToString(r["Name"]),
                    Qmax = r["Qmax"] == DBNull.Value ? 0 : Convert.ToDouble(r["Qmax"]),
                    SocStepPercent = r["SocStepPercent"] == DBNull.Value ? 0 : Convert.ToDouble(r["SocStepPercent"]),
                    DischargeCurrent_OCV = r["DischargeCurrent_OCV"] == DBNull.Value ? 0 : Convert.ToDouble(r["DischargeCurrent_OCV"]),
                    RestTime_OCV = r["RestTime_OCV"] == DBNull.Value ? 0 : Convert.ToDouble(r["RestTime_OCV"]),
                    DischargeCutoffVoltage_OCV = r["DischargeCutoffVoltage_OCV"] == DBNull.Value ? 0 : Convert.ToDouble(r["DischargeCutoffVoltage_OCV"]),
                });
            }
            return list;
        }
        /// <inheritdoc />
        public void Save(OCVProfile p, int cellId)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            if (p.Id == 0)
            {
                p.Id = ProfileIdProvider.GetNextId(conn);
                var sql = @"INSERT INTO OcvProfiles(Id, CellId, Name, Qmax, SocStepPercent, DischargeCurrent_OCV, RestTime_OCV, DischargeCutoffVoltage_OCV)
                            VALUES (@Id, @CellId, @Name, @Qmax, @Soc, @Cur, @Rest, @CutV);";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New OCV");
                cmd.Parameters.AddWithValue("@Qmax", (object?)p.Qmax ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Soc", (object?)p.SocStepPercent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cur", (object?)p.DischargeCurrent_OCV ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Rest", (object?)p.RestTime_OCV ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutV", (object?)p.DischargeCutoffVoltage_OCV ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE OcvProfiles SET Name=@Name, Qmax=@Qmax, SocStepPercent=@Soc, DischargeCurrent_OCV=@Cur,
                            RestTime_OCV=@Rest, DischargeCutoffVoltage_OCV=@CutV WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "OCV");
                cmd.Parameters.AddWithValue("@Qmax", (object?)p.Qmax ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Soc", (object?)p.SocStepPercent ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cur", (object?)p.DischargeCurrent_OCV ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Rest", (object?)p.RestTime_OCV ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CutV", (object?)p.DischargeCutoffVoltage_OCV ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        /// <inheritdoc />
        public void Delete(OCVProfile p)
        {
            if (p?.Id <= 0) return;
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM OcvProfiles WHERE Id=@Id;", conn);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
