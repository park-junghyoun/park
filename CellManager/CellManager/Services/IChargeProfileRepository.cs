using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>Persists charge profile templates per cell.</summary>
    public interface IChargeProfileRepository
    {
        ObservableCollection<ChargeProfile> Load(int cellId);
        void Save(ChargeProfile profile, int cellId);
        void Delete(ChargeProfile profile);
    }
}
