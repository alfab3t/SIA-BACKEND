using astratech_apps_backend.DTOs.Identity;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class EmployeeIdentityRepository : IEmployeeIdentityRepository
    {
        private readonly string _conn;

        public EmployeeIdentityRepository(IConfiguration config)
        {
            _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
                config.GetConnectionString("DefaultConnection")!, 
                Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING"));
        }

        public async Task<EmployeeIdentityResponse?> GetEmployeeIdentityByUserAsync(string username)
        {
            try
            {
                await using var conn = new SqlConnection(_conn);
                
                // Use direct query instead of stored procedure for now
                var query = @"
                    SELECT a.kry_username, a.jab_main_id, a.str_main_id, b.rol_id, a.kry_id
                    FROM ess_mskaryawan a 
                    RIGHT JOIN sso_msuser b ON a.kry_username = b.usr_id 
                    WHERE b.usr_id = @username
                ";
                
                await using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new EmployeeIdentityResponse
                    {
                        KryUsername = GetSafeString(reader, "kry_username"),
                        JabMainId = GetSafeString(reader, "jab_main_id"),
                        StrMainId = GetSafeString(reader, "str_main_id"),
                        RolId = GetSafeString(reader, "rol_id"),
                        KryId = GetSafeString(reader, "kry_id")
                    };
                }

                return new EmployeeIdentityResponse
                {
                    ErrorMessage = "Employee identity not found"
                };
            }
            catch (Exception ex)
            {
                return new EmployeeIdentityResponse
                {
                    ErrorMessage = $"Error retrieving employee identity: {ex.Message}"
                };
            }
        }

        private static string GetSafeString(SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return string.Empty;

                var value = reader.GetValue(ordinal);
                return value?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}