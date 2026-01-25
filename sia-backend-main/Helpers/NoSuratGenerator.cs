#nullable disable
using Microsoft.Data.SqlClient;
using System.Data;

namespace astratech_apps_backend.Helpers
{
    public class NoSuratGenerator
    {
        private readonly string _conn;

        public NoSuratGenerator(string connectionString)
        {
            _conn = connectionString;
        }

        /// <summary>
        /// Generate nomor surat berdasarkan format dari database
        /// </summary>
        /// <param name="jenisSuratId">ID jenis surat</param>
        /// <param name="konsentrasiId">ID konsentrasi (optional)</param>
        /// <returns>Nomor surat yang sudah di-generate</returns>
        public async Task<string> GenerateNoSuratAsync(string jenisSuratId, string konsentrasiId = null)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            // 1. Get format nomor surat
            string formatNoSurat = await GetFormatNoSuratAsync(conn, jenisSuratId);
            if (string.IsNullOrEmpty(formatNoSurat))
                throw new Exception("Format nomor surat tidak ditemukan");

            // 2. Get konsentrasi singkatan jika ada
            string konsentrasiSingkatan = null;
            if (!string.IsNullOrEmpty(konsentrasiId))
            {
                konsentrasiSingkatan = await GetKonsentrasiSingkatanAsync(conn, konsentrasiId);
            }

            // 3. Generate nomor urut (XXX)
            string nomorUrut = await GetNextNomorUrutAsync(conn, formatNoSurat);

            // 4. Get bulan romawi (MM)
            string bulanRomawi = ConvertToRoman(DateTime.Now.Month);

            // 5. Get tahun (YYYY)
            string tahun = DateTime.Now.Year.ToString();

            // 6. Replace placeholders
            string nomorSurat = formatNoSurat;
            nomorSurat = nomorSurat.Replace("XXX", nomorUrut);
            nomorSurat = nomorSurat.Replace("MM", bulanRomawi);
            nomorSurat = nomorSurat.Replace("YYYY", tahun);
            
            if (!string.IsNullOrEmpty(konsentrasiSingkatan))
            {
                nomorSurat = nomorSurat.Replace("PPP", konsentrasiSingkatan.Trim());
            }

            return nomorSurat;
        }

        private async Task<string> GetFormatNoSuratAsync(SqlConnection conn, string jenisSuratId)
        {
            using var cmd = new SqlCommand(
                "SELECT jsu_format_no FROM sia_msjenissurat WHERE jsu_id = @jsu_id", 
                conn);
            cmd.Parameters.AddWithValue("@jsu_id", jenisSuratId);
            
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        private async Task<string> GetKonsentrasiSingkatanAsync(SqlConnection conn, string konsentrasiId)
        {
            using var cmd = new SqlCommand(
                "SELECT kon_singkatan FROM sia_mskonsentrasi WHERE kon_id = @kon_id", 
                conn);
            cmd.Parameters.AddWithValue("@kon_id", konsentrasiId);
            
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        private async Task<string> GetNextNomorUrutAsync(SqlConnection conn, string formatNoSurat)
        {
            // Cek apakah sudah ada nomor surat dengan format ini
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 srt_no 
                FROM sia_mssurat a
                INNER JOIN sia_msjenissurat b ON a.jsu_id = b.jsu_id 
                WHERE b.jsu_format_no = @format
                ORDER BY a.srt_created_date DESC, a.srt_no DESC", 
                conn);
            cmd.Parameters.AddWithValue("@format", formatNoSurat);
            
            var lastNo = await cmd.ExecuteScalarAsync();
            
            if (lastNo != null)
            {
                string lastNoStr = lastNo.ToString();
                
                // Ambil 3 digit pertama
                int lastNumber = int.Parse(lastNoStr.Substring(0, 3));
                
                // Ambil 4 digit tahun terakhir
                string lastYear = lastNoStr.Substring(lastNoStr.Length - 4);
                string currentYear = DateTime.Now.Year.ToString();
                
                // Jika tahun sama, increment nomor
                if (lastYear == currentYear)
                {
                    lastNumber++;
                }
                else
                {
                    // Tahun berbeda, reset ke 1
                    lastNumber = 1;
                }
                
                // Format jadi 3 digit dengan leading zero
                return lastNumber.ToString("D3");
            }
            
            // Belum ada nomor surat, mulai dari 001
            return "001";
        }

        /// <summary>
        /// Convert angka ke romawi
        /// </summary>
        private string ConvertToRoman(int number)
        {
            if (number < 1 || number > 12) return number.ToString();
            
            string[] romanNumerals = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII" };
            return romanNumerals[number];
        }

        /// <summary>
        /// Generate nomor SK untuk file upload (tanpa insert ke database)
        /// Khusus untuk penamaan file SK yang di-upload
        /// </summary>
        public async Task<string> GenerateNoSKForFileAsync(string jenisSuratId, string konsentrasiId = null)
        {
            // Generate nomor surat tapi tidak insert ke database
            // Hanya untuk penamaan file
            return await GenerateNoSuratAsync(jenisSuratId, konsentrasiId);
        }
    }
}
