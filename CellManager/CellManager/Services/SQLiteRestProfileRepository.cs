using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>SQLite repository for storing rest profiles.</summary>
    public class SQLiteRestProfileRepository : IRestProfileRepository
    {
        private readonly string _dbPath;
        public SQLiteRestProfileRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "test_profiles.db");
            Initialize();
        }
        /// <summary>Creates the database file and rest profile schema if needed.</summary>
        private void Initialize()
        {
            if (!File.Exists(_dbPath)) SQLiteConnection.CreateFile(_dbPath);
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = @"
                CREATE TABLE IF NOT EXISTS RestProfiles(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CellId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    RestTimeSeconds REAL
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Migration: ensure RestTimeSeconds column exists and migrate data if needed
            using var pragma = new SQLiteCommand("PRAGMA table_info(RestProfiles);", conn);
            using var reader = pragma.ExecuteReader();
            var hasTimeSeconds = false;
            var hasOldTime = false;
            while (reader.Read())
            {
                var name = Convert.ToString(reader["name"]);
                if (string.Equals(name, "RestTimeSeconds", StringComparison.OrdinalIgnoreCase)) hasTimeSeconds = true;
                if (string.Equals(name, "RestTime", StringComparison.OrdinalIgnoreCase)) hasOldTime = true;
            }
            if (!hasTimeSeconds)
            {
                using var alter = new SQLiteCommand("ALTER TABLE RestProfiles ADD COLUMN RestTimeSeconds REAL;", conn);
                alter.ExecuteNonQuery();
                if (hasOldTime)
                {
                    using var update = new SQLiteCommand("UPDATE RestProfiles SET RestTimeSeconds = RestTime;", conn);
                    update.ExecuteNonQuery();
                }
            }
        }
        /// <inheritdoc />
        public ObservableCollection<RestProfile> Load(int cellId)
        {
            var list = new ObservableCollection<RestProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, RestTimeSeconds FROM RestProfiles WHERE CellId=@CellId ORDER BY Name;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CellId", cellId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new RestProfile
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Name = Convert.ToString(r["Name"]),
                    RestTime = r["RestTimeSeconds"] == DBNull.Value ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(r["RestTimeSeconds"])),
                });
            }
            return list;
        }
        /// <inheritdoc />
        public void Save(RestProfile p, int cellId)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            if (p.Id == 0)
            {
                p.Id = ProfileIdProvider.GetNextId(conn);
                var sql = @"INSERT INTO RestProfiles(Id, CellId, Name, RestTimeSeconds)
                            VALUES (@Id, @CellId, @Name, @Rest);";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New Rest");
                cmd.Parameters.AddWithValue("@Rest", p.RestTime == TimeSpan.Zero ? (object)DBNull.Value : p.RestTime.TotalSeconds);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE RestProfiles SET Name=@Name, RestTimeSeconds=@Rest WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "Rest");
                cmd.Parameters.AddWithValue("@Rest", p.RestTime == TimeSpan.Zero ? (object)DBNull.Value : p.RestTime.TotalSeconds);
                cmd.ExecuteNonQuery();
            }
        }
        /// <inheritdoc />
        public void Delete(RestProfile p)
        {
            if (p?.Id <= 0) return;
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM RestProfiles WHERE Id=@Id;", conn);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
