using astratech_apps_backend.DTOs.Mahasiswa;

namespace astratech_apps_backend.Repositories.Interfaces
{
    public interface IMahasiswaRepository
    {
        /// <summary>
        /// Mendapatkan detail profil mahasiswa berdasarkan mhs_id
        /// </summary>
        /// <param name="mhsId">ID Mahasiswa</param>
        /// <returns>Detail profil mahasiswa lengkap</returns>
        Task<MahasiswaDetailResponse?> GetDetailAsync(string mhsId);
    }
}