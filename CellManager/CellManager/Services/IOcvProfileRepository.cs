using System.Collections.ObjectModel;
using CellManager.Models.TestProfile;

namespace CellManager.Services
{
    /// <summary>Manages persistence for OCV profile templates.</summary>
    public interface IOcvProfileRepository
    {
        ObservableCollection<OCVProfile> Load(int cellId);
        void Save(OCVProfile profile, int cellId);
        void Delete(OCVProfile profile);
    }
}
