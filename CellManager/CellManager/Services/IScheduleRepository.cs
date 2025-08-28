using System.Collections.Generic;
using CellManager.Models;

namespace CellManager.Services
{
    public interface IScheduleRepository
    {
        List<Schedule> GetAll();
        Schedule? GetById(int id);
        void Save(Schedule schedule);
        void Delete(int id);
    }
}