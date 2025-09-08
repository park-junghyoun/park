using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
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
                    RestTime REAL
                );";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
        public ObservableCollection<RestProfile> Load(int cellId)
        {
            var list = new ObservableCollection<RestProfile>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            var sql = "SELECT Id, Name, RestTime FROM RestProfiles WHERE CellId=@CellId ORDER BY Name;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CellId", cellId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new RestProfile
                {
                    Id = Convert.ToInt32(r["Id"]),
                    Name = Convert.ToString(r["Name"]),
                    RestTime = r["RestTime"] == DBNull.Value ? 0 : Convert.ToDouble(r["RestTime"]),
                });
            }
            return list;
        }
        public void Save(RestProfile p, int cellId)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            if (p.Id == 0)
            {
                p.Id = ProfileIdProvider.GetNextId(conn);
                var sql = @"INSERT INTO RestProfiles(Id, CellId, Name, RestTime)
                            VALUES (@Id, @CellId, @Name, @Rest);";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "New Rest");
                cmd.Parameters.AddWithValue("@Rest", (object?)p.RestTime ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE RestProfiles SET Name=@Name, RestTime=@Rest WHERE Id=@Id;";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.Parameters.AddWithValue("@Name", p.Name ?? "Rest");
                cmd.Parameters.AddWithValue("@Rest", (object?)p.RestTime ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
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
