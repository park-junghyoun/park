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
                        RepeatCount INTEGER DEFAULT 1,
                        LoopStartIndex INTEGER DEFAULT 0,
                        LoopEndIndex INTEGER DEFAULT 0,
                        UNIQUE(CellId, Name)
                    );";
                using var cmd = new SQLiteCommand(createTableSql, conn);
                cmd.ExecuteNonQuery();

                var existing = new HashSet<string>();
                using (var infoCmd = new SQLiteCommand("PRAGMA table_info(Schedules);", conn))
                using (var reader = infoCmd.ExecuteReader())
                    while (reader.Read())
                        existing.Add(Convert.ToString(reader["name"]));

                var migrations = new (string Column, string Definition)[]
                {
                    ("CellId", "INTEGER DEFAULT 0"),
                    ("RepeatCount", "INTEGER DEFAULT 1"),
                    ("LoopStartIndex", "INTEGER DEFAULT 0"),
                    ("LoopEndIndex", "INTEGER DEFAULT 0")
                };

                foreach (var (column, definition) in migrations)
                {
                    if (!existing.Contains(column))
                    {
                        using var addCmd = new SQLiteCommand($"ALTER TABLE Schedules ADD COLUMN {column} {definition};", conn);
                        addCmd.ExecuteNonQuery();
                    }
                }
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
                string sql = "SELECT Id, CellId, Name, TestProfileIds, Ordering, Notes, RepeatCount, LoopStartIndex, LoopEndIndex FROM Schedules WHERE CellId = @CellId ORDER BY Ordering";
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
                        Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RepeatCount = reader.IsDBNull(6) ? 1 : reader.GetInt32(6),
                        LoopStartIndex = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                        LoopEndIndex = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load schedules failed: {ex.Message}");
            }
            return schedules;
        }

        public int Save(int cellId, Schedule schedule)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                var idsText = string.Join(",", schedule.TestProfileIds);

                if (schedule.Id == 0)
                {
                    const string insertSql = @"INSERT INTO Schedules (CellId, Name, TestProfileIds, Ordering, Notes, RepeatCount, LoopStartIndex, LoopEndIndex) VALUES (@CellId, @Name, @TestProfileIds, @Ordering, @Notes, @RepeatCount, @LoopStartIndex, @LoopEndIndex);";
                    using var cmd = new SQLiteCommand(insertSql, conn);
                    cmd.Parameters.AddWithValue("@CellId", cellId);
                    cmd.Parameters.AddWithValue("@Name", schedule.Name);
                    cmd.Parameters.AddWithValue("@TestProfileIds", idsText);
                    cmd.Parameters.AddWithValue("@Ordering", schedule.Ordering);
                    cmd.Parameters.AddWithValue("@Notes", (object?)schedule.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepeatCount", schedule.RepeatCount);
                    cmd.Parameters.AddWithValue("@LoopStartIndex", schedule.LoopStartIndex);
                    cmd.Parameters.AddWithValue("@LoopEndIndex", schedule.LoopEndIndex);
                    cmd.ExecuteNonQuery();
                    using var idCmd = new SQLiteCommand("SELECT last_insert_rowid();", conn);
                    schedule.Id = Convert.ToInt32(idCmd.ExecuteScalar());
                    schedule.CellId = cellId;
                }
                else
                {
                    const string updateSql = @"UPDATE Schedules SET Name = @Name, TestProfileIds = @TestProfileIds, Ordering = @Ordering, Notes = @Notes, RepeatCount = @RepeatCount, LoopStartIndex = @LoopStartIndex, LoopEndIndex = @LoopEndIndex WHERE Id = @Id AND CellId = @CellId";
                    using var cmd = new SQLiteCommand(updateSql, conn);
                    cmd.Parameters.AddWithValue("@Id", schedule.Id);
                    cmd.Parameters.AddWithValue("@CellId", cellId);
                    cmd.Parameters.AddWithValue("@Name", schedule.Name);
                    cmd.Parameters.AddWithValue("@TestProfileIds", idsText);
                    cmd.Parameters.AddWithValue("@Ordering", schedule.Ordering);
                    cmd.Parameters.AddWithValue("@Notes", (object?)schedule.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RepeatCount", schedule.RepeatCount);
                    cmd.Parameters.AddWithValue("@LoopStartIndex", schedule.LoopStartIndex);
                    cmd.Parameters.AddWithValue("@LoopEndIndex", schedule.LoopEndIndex);
                    cmd.ExecuteNonQuery();
                }
                return schedule.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save schedule failed: {ex.Message}");
                return 0;
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