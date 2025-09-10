using System.Threading.Tasks;

namespace CellManager.Services
{
    public interface IServerStatusService
    {
        Task<bool> IsServerAvailableAsync();
    }
}

