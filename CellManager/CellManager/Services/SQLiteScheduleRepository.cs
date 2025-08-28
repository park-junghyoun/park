using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using CellManager.Models;

namespace CellManager.Services
{
    public class SQLiteScheduleRepository : IScheduleRepository
    {
        private const string ScheduleDbFileName = "schedules.db";
        private readonly string _dbPath;

        public SQLiteScheduleRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, ScheduleDbFileName);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                }

                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Schedules (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        TestProfileIds TEXT,
                        Ordering INTEGER,
                        Notes TEXT
                    );";
                using var cmd = new SQLiteCommand(createTableSql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Schedule database initialization failed: {ex.Message}");
            }
        }

        public List<Schedule> GetAll()
        {
            var schedules = new List<Schedule>();
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                string sql = "SELECT * FROM Schedules ORDER BY Ordering";
                using var cmd = new SQLiteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var idsText = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    var ids = idsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(int.Parse)
                                     .ToList();
                    schedules.Add(new Schedule
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        TestProfileIds = ids,
                        Ordering = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Notes = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAll schedules failed: {ex.Message}");
            }
            return schedules;
        }

        public Schedule? GetById(int id)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                string sql = "SELECT * FROM Schedules WHERE Id = @Id";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var idsText = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    var ids = idsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(int.Parse)
                                     .ToList();
                    return new Schedule
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        TestProfileIds = ids,
                        Ordering = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Notes = reader.IsDBNull(4) ? null : reader.GetString(4)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get schedule by id failed: {ex.Message}");
            }
            return null;
        }

        public void Save(Schedule schedule)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                var idsText = string.Join(",", schedule.TestProfileIds);

                if (schedule.Id == 0)
                {
                    string insertSql = @"INSERT INTO Schedules (Name, TestProfileIds, Ordering, Notes) VALUES (@Name, @TestProfileIds, @Ordering, @Notes);";
                    using var cmd = new SQLiteCommand(insertSql, conn);
                    cmd.Parameters.AddWithValue("@Name", schedule.Name);
                    cmd.Parameters.AddWithValue("@TestProfileIds", idsText);
                    cmd.Parameters.AddWithValue("@Ordering", schedule.Ordering);
                    cmd.Parameters.AddWithValue("@Notes", (object?)schedule.Notes ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    schedule.Id = (int)conn.LastInsertRowId;
                }
                else
                {
                    string updateSql = @"UPDATE Schedules SET Name = @Name, TestProfileIds = @TestProfileIds, Ordering = @Ordering, Notes = @Notes WHERE Id = @Id";
                    using var cmd = new SQLiteCommand(updateSql, conn);
                    cmd.Parameters.AddWithValue("@Id", schedule.Id);
                    cmd.Parameters.AddWithValue("@Name", schedule.Name);
                    cmd.Parameters.AddWithValue("@TestProfileIds", idsText);
                    cmd.Parameters.AddWithValue("@Ordering", schedule.Ordering);
                    cmd.Parameters.AddWithValue("@Notes", (object?)schedule.Notes ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save schedule failed: {ex.Message}");
            }
        }

        public void Delete(int id)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                string sql = "DELETE FROM Schedules WHERE Id = @Id";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete schedule failed: {ex.Message}");
            }
        }
    }
}