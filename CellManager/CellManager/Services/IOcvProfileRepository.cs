using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    public interface IOcvProfileRepository
    {
        ObservableCollection<OCVProfile> Load(int cellId);
        void Save(OCVProfile profile, int cellId);
        void Delete(OCVProfile profile);
    }
}
