using System.Collections.Generic;
using CellManager.Models;

namespace CellManager.Services
{
    public interface IScheduleRepository
    {
        List<Schedule> Load(int cellId);
        void Save(int cellId, Schedule schedule);
        void Delete(int cellId, int id);
    }
}

