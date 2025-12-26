# Perbaikan GetAll Meninggal Dunia - Summary

## Masalah
- Endpoint `/api/MeninggalDunia/GetAll` mengembalikan data kosong meskipun bisa diakses tanpa parameter
- Endpoint `/api/MeninggalDunia/Riwayat` juga mengembalikan data kosong tanpa parameter

## Perbaikan yang Dilakukan

### 1. Controller (MeninggalDuniaController.cs)
- ✅ Endpoint `[HttpGet("GetAll")]` bisa dipanggil tanpa parameter
- ✅ Endpoint `[HttpGet("Riwayat")]` bisa dipanggil tanpa parameter

### 2. Repository (MeninggalDuniaRepository.cs)
- ✅ **GetAllAsync**: Mengganti stored procedure dengan query SQL langsung
- ✅ **GetRiwayatAsync**: Mengganti stored procedure dengan query SQL langsung
- ✅ Menggunakan LEFT JOIN untuk memastikan data tetap muncul meski relasi tidak lengkap
- ✅ Menambahkan filter kondisional berdasarkan parameter yang diberikan
- ✅ Memperbaiki mapping field `NamaMahasiswa` dan `Prodi`

## Endpoint yang Tersedia

### 1. GetAll - Semua Data
```
GET /api/MeninggalDunia/GetAll
GET /api/MeninggalDunia/GetAll?status=Draft&pageSize=20
```

### 2. Riwayat - Data yang Sudah Disetujui/Ditolak
```
GET /api/MeninggalDunia/Riwayat
GET /api/MeninggalDunia/Riwayat?keyword=nama&pageSize=20
```

## Keuntungan Perbaikan
- ✅ Data bisa diakses tanpa parameter
- ✅ Tidak bergantung pada stored procedure yang kompleks
- ✅ Query lebih transparan dan mudah di-debug
- ✅ Performa lebih baik dengan LEFT JOIN
- ✅ Filter tetap berfungsi jika diperlukan

## Testing
Setelah perbaikan, test dengan:
1. `GET /api/MeninggalDunia/GetAll` - semua data kecuali dihapus
2. `GET /api/MeninggalDunia/Riwayat` - data yang sudah diproses (bukan Draft/Dihapus)