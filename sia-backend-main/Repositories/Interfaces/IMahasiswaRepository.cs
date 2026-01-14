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

        /// <summary>
        /// Mendapatkan daftar konsentrasi berdasarkan username sekprodi
        /// </summary>
        /// <param name="username">Username sekprodi</param>
        /// <returns>List konsentrasi dengan id dan nama</returns>
        Task<List<KonsentrasiListResponse>> GetKonsentrasiListBySekprodiAsync(string username);

        /// <summary>
        /// Mendapatkan daftar mahasiswa berdasarkan konsentrasi
        /// </summary>
        /// <param name="konId">ID Konsentrasi</param>
        /// <returns>List mahasiswa dengan id dan nama</returns>
        Task<List<MahasiswaByKonsentrasiResponse>> GetMahasiswaByKonsentrasiAsync(string konId);

        /// <summary>
        /// Mendapatkan data mahasiswa berdasarkan NIM
        /// </summary>
        /// <param name="nim">NIM Mahasiswa</param>
        /// <returns>Data mahasiswa dengan nama, konsentrasi, angkatan, dan kelas</returns>
        Task<MahasiswaByNIMResponse?> GetMahasiswaByNIMAsync(string nim);

        /// <summary>
        /// Check bebas tanggungan mahasiswa
        /// </summary>
        /// <param name="userId">User ID / NIM Mahasiswa</param>
        /// <returns>Status bebas tanggungan: "OK" atau "NOK"</returns>
        Task<string> CheckBebasTanggunganAsync(string userId);
    }
}