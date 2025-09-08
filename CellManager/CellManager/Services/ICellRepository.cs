using System.Collections.Generic;
using CellManager.Models;

namespace CellManager.Services
{
    public interface ICellRepository
    {
        List<Cell> LoadCells();
        void SaveCell(Cell cell);
        void DeleteCell(Cell cell);
        int GetNextCellId();
    }
}