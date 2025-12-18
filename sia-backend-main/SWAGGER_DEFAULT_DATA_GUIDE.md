# Panduan Swagger - Menampilkan Semua Data Secara Default

## ✅ Yang Sudah Diperbaiki

### 1. Endpoint Utama: GET /api/CutiAkademik
- ✅ **Sekarang menampilkan SEMUA data secara default** tanpa perlu mengisi parameter
- ✅ Ketika parameter `status` kosong → menampilkan semua data yang tidak dihapus
- ✅ Ketika parameter `status` diisi → menggunakan filter sesuai status

### 2. Endpoint Riwayat: GET /api/CutiAkademik/riwayat  
- ✅ **Sudah menampilkan SEMUA data secara default** tanpa perlu mengisi parameter
- ✅ Ketika parameter `status` kosong → menampilkan semua data yang tidak dihapus
- ✅ Ketika parameter `status` diisi → menggunakan filter sesuai status

## 🚀 Cara Testing di Swagger

### 1. Jalankan Aplikasi
```bash
cd sia-backend-main
dotnet run
```

### 2. Buka Swagger
```
http://localhost:5234/swagger
```

### 3. Test Endpoint Utama (GET /api/CutiAkademik)

#### Skenario 1: Tampilkan Semua Data (DEFAULT)
1. Expand **GET /api/CutiAkademik**
2. Klik **"Try it out"**
3. **JANGAN ISI PARAMETER APAPUN** (biarkan semua kosong)
4. Klik **"Execute"**
5. ✅ **Hasilnya: Semua data cuti akademik akan muncul**

#### Skenario 2: Filter Berdasarkan Status
1. Expand **GET /api/CutiAkademik**
2. Klik **"Try it out"**
3. Isi parameter `status` dengan: `disetujui`
4. Klik **"Execute"**
5. ✅ **Hasilnya: Hanya data dengan status "Disetujui"**

### 4. Test Endpoint Riwayat (GET /api/CutiAkademik/riwayat)

#### Skenario 1: Tampilkan Semua Riwayat (DEFAULT)
1. Expand **GET /api/CutiAkademik/riwayat**
2. Klik **"Try it out"**
3. **JANGAN ISI PARAMETER APAPUN** (biarkan semua kosong)
4. Klik **"Execute"**
5. ✅ **Hasilnya: Semua riwayat cuti akademik akan muncul**

#### Skenario 2: Filter Riwayat Berdasarkan Status
1. Expand **GET /api/CutiAkademik/riwayat**
2. Klik **"Try it out"**
3. Isi parameter `status` dengan: `disetujui`
4. Klik **"Execute"**
5. ✅ **Hasilnya: Hanya riwayat dengan status "Disetujui"**

## 📋 Contoh Response yang Akan Muncul

Ketika Anda klik **Execute** tanpa mengisi parameter apapun, Anda akan melihat response seperti ini:

```json
[
  {
    "id": "012/PMA/CA/IX/2025",
    "idDisplay": "012/PMA/CA/IX/2025",
    "mhsId": "0420240032",
    "tahunAjaran": "2024/2025",
    "semester": "Genap",
    "approveProdi": "andreas_e",
    "approveDir1": "tonny.pongoh",
    "tanggal": "11 Sep 2025",
    "suratNo": "008/PA-WADIR-I/SKC/IX/2025",
    "status": "Disetujui"
  },
  {
    "id": "011/PMA/CA/IX/2025",
    "idDisplay": "011/PMA/CA/IX/2025",
    "mhsId": "0420240067",
    "tahunAjaran": "2024/2025",
    "semester": "Genap",
    "approveProdi": "andreas_e",
    "approveDir1": "tonny.pongoh",
    "tanggal": "07 Sep 2025",
    "suratNo": "010/PA-WADIR-I/SKC/IX/2025",
    "status": "Disetujui"
  }
]
```

## 🎯 Perbedaan Kedua Endpoint

### GET /api/CutiAkademik (Endpoint Utama)
- **Tujuan**: Data cuti akademik dengan berbagai filter role-based
- **Default**: Menampilkan semua data yang tidak dihapus
- **Filter**: Mendukung mhsId, status, userId, role, search

### GET /api/CutiAkademik/riwayat (Endpoint Riwayat)
- **Tujuan**: Riwayat cuti akademik untuk keperluan laporan
- **Default**: Menampilkan semua data yang tidak dihapus
- **Filter**: Mendukung userId, status, search

## 🔧 Troubleshooting

### Jika Masih Mendapat Response Kosong []

1. **Cek Database**: Pastikan ada data di tabel `sia_mscutiakademik`
2. **Cek Connection String**: Pastikan koneksi database benar
3. **Cek Environment Variable**: Pastikan `DECRYPT_KEY_CONNECTION_STRING` sudah diset
4. **Restart Aplikasi**: Stop dan jalankan ulang `dotnet run`

### Jika Mendapat Error 500

1. **Lihat Console Log**: Cek error message di terminal
2. **Cek Database Connection**: Pastikan database server berjalan
3. **Cek Stored Procedure**: Pastikan SP `sia_getDataCutiAkademik` ada di database

## 📝 Tips Penggunaan

1. **Mulai dengan parameter kosong** untuk melihat semua data
2. **Gunakan endpoint riwayat** untuk laporan dan export
3. **Gunakan endpoint utama** untuk operasional sehari-hari
4. **Kombinasikan parameter** untuk filter yang lebih spesifik
5. **Perhatikan Response Code 200** = berhasil

## 🎉 Hasil Akhir

Sekarang ketika Anda:
- Buka Swagger → Pilih endpoint → Klik "Try it out" → Langsung "Execute"
- **TIDAK PERLU mengisi parameter apapun**
- **Semua data akan langsung muncul!**

Kedua endpoint (`/api/CutiAkademik` dan `/api/CutiAkademik/riwayat`) sekarang akan menampilkan semua data secara default tanpa perlu mengisi parameter status atau lainnya.