using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    public interface IChargeProfileRepository
    {
        ObservableCollection<ChargeProfile> Load(int cellId);
        void Save(ChargeProfile profile, int cellId);
        void Delete(ChargeProfile profile);
    }
}
