using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>Handles persistence for rest interval profiles.</summary>
    public interface IRestProfileRepository
    {
        ObservableCollection<RestProfile> Load(int cellId);
        void Save(RestProfile profile, int cellId);
        void Delete(RestProfile profile);
    }
}
