using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CellManager.Services
{
    /// <summary>Simple availability probe against the configured MySQL control database.</summary>
    public class MySqlServerStatusService : IServerStatusService
    {
        private const string DefaultConnectionString = "Server=localhost;User Id=root;Password=;Database=cell_manager;";

        /// <inheritdoc />
        public async Task<bool> IsServerAvailableAsync()
        {
            try
            {
                using var conn = new MySqlConnection(DefaultConnectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

