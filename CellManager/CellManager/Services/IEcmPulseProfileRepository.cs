using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    public interface IEcmPulseProfileRepository
    {
        ObservableCollection<ECMPulseProfile> Load(int cellId);
        void Save(ECMPulseProfile profile, int cellId);
        void Delete(ECMPulseProfile profile);
    }
}
