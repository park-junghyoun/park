using System.Collections.ObjectModel;
using CellManager.Models;

namespace CellManager.Services
{
    public interface ITestProfileRepository
    {
        void SaveTestProfile(TestProfileModel profile);
        void DeleteTestProfile(TestProfileModel profile);
        ObservableCollection<TestProfileModel> LoadTestProfiles(int cellId);
    }
}