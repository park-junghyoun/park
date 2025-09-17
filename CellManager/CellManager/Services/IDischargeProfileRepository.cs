using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>Provides access to stored discharge profile definitions.</summary>
    public interface IDischargeProfileRepository
    {
        ObservableCollection<DischargeProfile> Load(int cellId);
        void Save(DischargeProfile profile, int cellId);
        void Delete(DischargeProfile profile);
    }
}
