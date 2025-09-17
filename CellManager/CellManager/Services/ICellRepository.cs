using System.Collections.Generic;
using CellManager.Models;

namespace CellManager.Services
{
    /// <summary>
    ///     Abstraction for CRUD operations against the cell library store.
    /// </summary>
    public interface ICellRepository
    {
        List<Cell> LoadCells();
        void SaveCell(Cell cell);
        void DeleteCell(Cell cell);
        int GetNextCellId();
    }
}