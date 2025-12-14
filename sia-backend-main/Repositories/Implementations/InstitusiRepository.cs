using astratech_apps_backend.DTOs.Institusi;
using astratech_apps_backend.Models;
using astratech_apps_backend.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Repositories.Implementations
{
    public class InstitusiRepository(IConfiguration config) : IInstitusiRepository
    {
        private readonly string _conn = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(config.GetConnectionString("DefaultConnection")!, Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING"));

        public async Task<int> CreateAsync(CreateInstitusiRequest dto, string createdBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_createInstitusi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@NamaInstitusi", dto.NamaInstitusi);
            cmd.Parameters.AddWithValue("@NamaDirektur", dto.NamaDirektur);
            cmd.Parameters.AddWithValue("@NamaWadir1", dto.NamaWadir1);
            cmd.Parameters.AddWithValue("@NamaWadir2", dto.NamaWadir2);
            cmd.Parameters.AddWithValue("@NamaWadir3", dto.NamaWadir3);
            cmd.Parameters.AddWithValue("@NamaWadir4", dto.NamaWadir4);
            cmd.Parameters.AddWithValue("@Alamat", dto.Alamat);
            cmd.Parameters.AddWithValue("@KodePos", dto.KodePos);
            cmd.Parameters.AddWithValue("@Telepon", dto.Telepon);
            cmd.Parameters.AddWithValue("@Fax", dto.Fax);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            cmd.Parameters.AddWithValue("@Website", dto.Website);
            cmd.Parameters.AddWithValue("@TanggalBerdiri", dto.TanggalBerdiri);
            cmd.Parameters.AddWithValue("@NomorSK", dto.NomorSK);
            cmd.Parameters.AddWithValue("@TanggalSK", dto.TanggalSK);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            await conn.OpenAsync();
            var newId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task<(IEnumerable<Institusi>, int totalData)> GetAllAsync(GetAllInstitusiRequest dto)
        {
            var list = new List<Institusi>();
            int totalData = 0;

            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_getDataInstitusi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Keyword", dto.SearchKeyword);
            cmd.Parameters.AddWithValue("@Status", dto.Status);
            cmd.Parameters.AddWithValue("@Urut", dto.Urut);
            cmd.Parameters.AddWithValue("@Halaman", dto.PageNumber);
            cmd.Parameters.AddWithValue("@Limit", dto.PageSize);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                totalData = reader.GetInt32(reader.GetOrdinal("Count"));
                do
                {
                    list.Add(new Institusi
                    {
                        Id = reader.GetInt16(reader.GetOrdinal("ins_id")),
                        RowNumber = reader.GetInt64(reader.GetOrdinal("rownum")),
                        NamaInstitusi = reader.GetString(reader.GetOrdinal("ins_nama")),
                        NamaDirektur = reader.GetString(reader.GetOrdinal("ins_direktur")),
                        TanggalSK = reader.GetDateTime(reader.GetOrdinal("ins_tgl_sk")),
                        NomorSK = reader.GetString(reader.GetOrdinal("ins_no_sk")),
                        Status = reader.GetString(reader.GetOrdinal("ins_status")),
                    });
                } while (await reader.ReadAsync());
            }
            return (list, totalData);
        }

        public async Task<Institusi?> GetByIdAsync(short id)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_detailInstitusi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Institusi
                {
                    NamaInstitusi = reader.GetString(reader.GetOrdinal("ins_nama")),
                    NamaDirektur = reader.GetString(reader.GetOrdinal("ins_direktur")),
                    NamaWadir1 = reader.GetString(reader.GetOrdinal("ins_wadir1")),
                    NamaWadir2 = reader.GetString(reader.GetOrdinal("ins_wadir2")),
                    NamaWadir3 = reader.GetString(reader.GetOrdinal("ins_wadir3")),
                    NamaWadir4 = reader.GetString(reader.GetOrdinal("ins_wadir4")),
                    Alamat = reader.GetString(reader.GetOrdinal("ins_alamat")),
                    KodePos = reader.GetString(reader.GetOrdinal("ins_kodepos")),
                    Telepon = reader.GetString(reader.GetOrdinal("ins_telepon")),
                    Fax = reader.GetString(reader.GetOrdinal("ins_fax")),
                    Email = reader.GetString(reader.GetOrdinal("ins_email")),
                    Website = reader.GetString(reader.GetOrdinal("ins_website")),
                    TanggalBerdiri = reader.GetDateTime(reader.GetOrdinal("ins_tgl_berdiri")),
                    NomorSK = reader.GetString(reader.GetOrdinal("ins_no_sk")),
                    TanggalSK = reader.GetDateTime(reader.GetOrdinal("ins_tgl_sk")),
                    Status = reader.GetString(reader.GetOrdinal("ins_status")),
                };
            }
            return null;
        }

        public async Task<bool> SetStatusAsync(short id, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_setStatusInstitusi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

            // PASTIKAN [SET NOCOUNT ON] PADA STORED PROCEDURE DIHAPUS AGAR TERHITUNG ADA ROWS AFFECTEDNYA!

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAsync(UpdateInstitusiRequest dto, string updatedBy)
        {
            await using var conn = new SqlConnection(_conn);
            await using var cmd = new SqlCommand("sia_editInstitusi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", dto.Id);
            cmd.Parameters.AddWithValue("@NamaInstitusi", dto.NamaInstitusi);
            cmd.Parameters.AddWithValue("@NamaDirektur", dto.NamaDirektur);
            cmd.Parameters.AddWithValue("@NamaWadir1", dto.NamaWadir1);
            cmd.Parameters.AddWithValue("@NamaWadir2", dto.NamaWadir2);
            cmd.Parameters.AddWithValue("@NamaWadir3", dto.NamaWadir3);
            cmd.Parameters.AddWithValue("@NamaWadir4", dto.NamaWadir4);
            cmd.Parameters.AddWithValue("@Alamat", dto.Alamat);
            cmd.Parameters.AddWithValue("@KodePos", dto.KodePos);
            cmd.Parameters.AddWithValue("@Telepon", dto.Telepon);
            cmd.Parameters.AddWithValue("@Fax", dto.Fax);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            cmd.Parameters.AddWithValue("@Website", dto.Website);
            cmd.Parameters.AddWithValue("@TanggalBerdiri", dto.TanggalBerdiri);
            cmd.Parameters.AddWithValue("@NomorSK", dto.NomorSK);
            cmd.Parameters.AddWithValue("@TanggalSK", dto.TanggalSK);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

            // PASTIKAN [SET NOCOUNT ON] PADA STORED PROCEDURE DIHAPUS AGAR TERHITUNG ADA ROWS AFFECTEDNYA!

            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
