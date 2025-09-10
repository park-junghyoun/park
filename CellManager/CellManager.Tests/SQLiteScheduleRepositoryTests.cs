using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using CellManager.Models;
using CellManager.Services;
using Xunit;

namespace CellManager.Tests
{
    public class SQLiteScheduleRepositoryTests
    {
        [Fact]
        public void SaveAndLoad_IsolatedByCellId()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");
                Directory.CreateDirectory(dataDir);
                var dbPath = Path.Combine(dataDir, "schedules.db");
                if (File.Exists(dbPath)) File.Delete(dbPath);

                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    var sql = @"CREATE TABLE Schedules (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Name TEXT NOT NULL,
                                    TestProfileIds TEXT,
                                    Ordering INTEGER,
                                    Notes TEXT
                                );";
                    using var cmd = new SQLiteCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }

                var repo = new SQLiteScheduleRepository();
                var id1 = repo.Save(1, new Schedule { Name = "S1", Ordering = 1 });
                var id2 = repo.Save(2, new Schedule { Name = "S2", Ordering = 1 });

                Assert.NotEqual(0, id1);
                Assert.NotEqual(0, id2);

                var cell1 = repo.Load(1);
                var cell2 = repo.Load(2);

                Assert.Single(cell1);
                Assert.Equal("S1", cell1[0].Name);
                Assert.Equal(1, cell1[0].CellId);

                Assert.Single(cell2);
                Assert.Equal("S2", cell2[0].Name);
                Assert.Equal(2, cell2[0].CellId);
            }
            catch (DllNotFoundException)
            {
                // Skip if SQLite native library is unavailable in the test environment
            }
        }
    }
}
