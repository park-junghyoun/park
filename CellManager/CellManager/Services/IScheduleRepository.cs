using System.Collections.Generic;
using CellManager.Models;

namespace CellManager.Services
{
    /// <summary>Persists composed schedules and retrieves them for a given cell.</summary>
    public interface IScheduleRepository
    {
        List<Schedule> Load(int cellId);
        int Save(int cellId, Schedule schedule);
        void Delete(int cellId, int id);
    }
}

