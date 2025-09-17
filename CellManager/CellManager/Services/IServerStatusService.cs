using System.Threading.Tasks;

namespace CellManager.Services
{
    /// <summary>Exposes connectivity checks for the background control server.</summary>
    public interface IServerStatusService
    {
        Task<bool> IsServerAvailableAsync();
    }
}

