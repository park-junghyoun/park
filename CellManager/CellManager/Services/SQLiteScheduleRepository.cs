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
                        CellId INTEGER NOT NULL,
                        Name TEXT NOT NULL,
                        TestProfileIds TEXT,
                        Ordering INTEGER,
                        Notes TEXT,
                        UNIQUE(CellId, Name)
                    );";
                using var cmd = new SQLiteCommand(createTableSql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Schedule database initialization failed: {ex.Message}");
            }
        }

        public List<Schedule> Load(int cellId)
        {
            var schedules = new List<Schedule>();
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                string sql = "SELECT * FROM Schedules WHERE CellId = @CellId ORDER BY Ordering";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var idsText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                    var ids = idsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(int.Parse)
                                     .ToList();
                    schedules.Add(new Schedule
                    {
                        Id = reader.GetInt32(0),
                        CellId = reader.GetInt32(1),
                        Name = reader.GetString(2),
                        TestProfileIds = ids,
                        Ordering = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        Notes = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load schedules failed: {ex.Message}");
            }
            return schedules;
        }

        public void Save(int cellId, Schedule schedule)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                var idsText = string.Join(",", schedule.TestProfileIds);

                if (schedule.Id == 0)
                {
                    string insertSql = @"INSERT INTO Schedules (CellId, Name, TestProfileIds, Ordering, Notes) VALUES (@CellId, @Name, @TestProfileIds, @Ordering, @Notes);";
                    using var cmd = new SQLiteCommand(insertSql, conn);
                    cmd.Parameters.AddWithValue("@CellId", cellId);
                    cmd.Parameters.AddWithValue("@Name", schedule.Name);
                    cmd.Parameters.AddWithValue("@TestProfileIds", idsText);
                    cmd.Parameters.AddWithValue("@Ordering", schedule.Ordering);
                    cmd.Parameters.AddWithValue("@Notes", (object?)schedule.Notes ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    schedule.Id = (int)conn.LastInsertRowId;
                    schedule.CellId = cellId;
                }
                else
                {
                    string updateSql = @"UPDATE Schedules SET Name = @Name, TestProfileIds = @TestProfileIds, Ordering = @Ordering, Notes = @Notes WHERE Id = @Id AND CellId = @CellId";
                    using var cmd = new SQLiteCommand(updateSql, conn);
                    cmd.Parameters.AddWithValue("@Id", schedule.Id);
                    cmd.Parameters.AddWithValue("@CellId", cellId);
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

        public void Delete(int cellId, int id)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                string sql = "DELETE FROM Schedules WHERE Id = @Id AND CellId = @CellId";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CellId", cellId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete schedule failed: {ex.Message}");
            }
        }
    }
}