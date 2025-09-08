using System;
using System.Data.SQLite;

namespace CellManager.Services
{
    internal static class ProfileIdProvider
    {
        public static int GetNextId(SQLiteConnection conn)
        {
            const string sql = @"
                SELECT MAX(Id) FROM ChargeProfiles
                UNION ALL SELECT MAX(Id) FROM DischargeProfiles
                UNION ALL SELECT MAX(Id) FROM EcmPulseProfiles
                UNION ALL SELECT MAX(Id) FROM OcvProfiles
                UNION ALL SELECT MAX(Id) FROM RestProfiles;";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            var max = 0;
            while (reader.Read())
            {
                if (reader[0] != DBNull.Value)
                    max = Math.Max(max, Convert.ToInt32(reader[0]));
            }
            return max + 1;
        }
    }
}
