using System;
using CellManager.Models;
using CellManager.Services;
using Xunit;

namespace CellManager.Tests
{
    public class SQLiteCellRepositoryTests
    {
        [Fact]
        public void SaveCell_Throws_WhenCellIsNull()
        {
            var repo = new SQLiteCellRepository();
            Assert.Throws<ArgumentNullException>(() => repo.SaveCell(null));
        }

        [Fact]
        public void SaveCell_Throws_WhenModelNameMissing()
        {
            var repo = new SQLiteCellRepository();
            var cell = new Cell { RatedCapacity = 100, NominalVoltage = 3000 };
            Assert.Throws<ArgumentException>(() => repo.SaveCell(cell));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void SaveCell_Throws_WhenRatedCapacityInvalid(double capacity)
        {
            var repo = new SQLiteCellRepository();
            var cell = new Cell { ModelName = "A", RatedCapacity = capacity, NominalVoltage = 3000 };
            Assert.Throws<ArgumentException>(() => repo.SaveCell(cell));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void SaveCell_Throws_WhenNominalVoltageInvalid(double voltage)
        {
            var repo = new SQLiteCellRepository();
            var cell = new Cell { ModelName = "A", RatedCapacity = 100, NominalVoltage = voltage };
            Assert.Throws<ArgumentException>(() => repo.SaveCell(cell));
        }
    }
}
