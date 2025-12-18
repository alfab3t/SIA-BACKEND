# Dropdown Endpoints Guide - Prodi & Mahasiswa

## 📋 Overview

Endpoints untuk mendukung dropdown list Program Studi dan Mahasiswa di frontend.

## 🎯 Endpoints

### 1. Get Prodi List
**Endpoint:** `GET /api/Mahasiswa/GetProdiList`

**Deskripsi:** Membaca data unik kon_id dan kon_nama dari sia_msmahasiswa untuk menampilkan daftar Program Studi yang tersedia.

**Response:**
```json
[
  {
    "konId": "001",
    "konNama": "Teknik Informatika"
  },
  {
    "konId": "002", 
    "konNama": "Sistem Informasi"
  },
  {
    "konId": "003",
    "konNama": "Teknik Mesin"
  }
]
```

**SQL Query:**
```sql
SELECT DISTINCT k.kon_id, k.kon_nama
FROM sia_msmahasiswa m
INNER JOIN sia_mskonsentrasi k ON m.kon_id = k.kon_id
WHERE m.kon_id IS NOT NULL 
  AND k.kon_nama IS NOT NULL
ORDER BY k.kon_nama
```

### 2. Get Mahasiswa by Prodi
**Endpoint:** `GET /api/Mahasiswa/GetByProdi?konId={prodiId}`

**Parameters:**
- `konId` (required): ID Konsentrasi/Program Studi

**Deskripsi:** Membaca data mhs_id dan mhs_nama dari sia_msmahasiswa berdasarkan kon_id untuk menampilkan mahasiswa yang sesuai dengan prodi yang dipilih.

**Example Request:**
```
GET /api/Mahasiswa/GetByProdi?konId=001
```

**Response:**
```json
[
  {
    "mhsId": "0420240001",
    "mhsNama": "John Doe",
    "angkatan": "2024",
    "konNama": "Teknik Informatika"
  },
  {
    "mhsId": "0420240002",
    "mhsNama": "Jane Smith", 
    "angkatan": "2024",
    "konNama": "Teknik Informatika"
  }
]
```

**SQL Query:**
```sql
SELECT m.mhs_id, m.mhs_nama, m.mhs_angkatan, k.kon_nama
FROM sia_msmahasiswa m
INNER JOIN sia_mskonsentrasi k ON m.kon_id = k.kon_id
WHERE m.kon_id = @konId
  AND m.mhs_nama IS NOT NULL
ORDER BY m.mhs_nama
```

## 🧪 Testing

### Test Prodi List
```bash
curl -X GET "http://localhost:5234/api/Mahasiswa/GetProdiList"
```

### Test Mahasiswa by Prodi
```bash
curl -X GET "http://localhost:5234/api/Mahasiswa/GetByProdi?konId=001"
```

## 💡 Frontend Usage

### React/JavaScript Example
```javascript
// Get Prodi List
const getProdiList = async () => {
  try {
    const response = await fetch(`${API_LINK}Mahasiswa/GetProdiList`);
    const prodiList = await response.json();
    
    // Populate dropdown
    setProdiOptions(prodiList.map(prodi => ({
      value: prodi.konId,
      label: prodi.konNama
    })));
  } catch (error) {
    console.error('Error fetching prodi list:', error);
  }
};

// Get Mahasiswa by Prodi
const getMahasiswaByProdi = async (konId) => {
  try {
    const response = await fetch(`${API_LINK}Mahasiswa/GetByProdi?konId=${konId}`);
    const mahasiswaList = await response.json();
    
    // Populate dropdown
    setMahasiswaOptions(mahasiswaList.map(mhs => ({
      value: mhs.mhsId,
      label: `${mhs.mhsNama} (${mhs.mhsId})`
    })));
  } catch (error) {
    console.error('Error fetching mahasiswa list:', error);
  }
};

// Usage in component
const handleProdiChange = (selectedKonId) => {
  setSelectedProdi(selectedKonId);
  getMahasiswaByProdi(selectedKonId);
};
```

## 🔧 Implementation Notes

### Why No Stored Procedure?
- **Simple queries**: Data lookup tidak memerlukan business logic kompleks
- **Performance**: Direct SQL lebih cepat untuk query sederhana
- **Maintenance**: Lebih mudah di-maintain dan di-debug
- **Flexibility**: Mudah dimodifikasi sesuai kebutuhan frontend

### Database Tables Used
- `sia_msmahasiswa`: Tabel utama mahasiswa
- `sia_mskonsentrasi`: Tabel konsentrasi/program studi

### Error Handling
- Validation parameter input
- Database connection error handling
- Comprehensive logging untuk debugging

## 📊 Performance Considerations

- Query menggunakan `INNER JOIN` untuk memastikan data konsisten
- `ORDER BY` untuk hasil yang terurut
- `DISTINCT` untuk menghindari duplikasi prodi
- Index pada `kon_id` dan `mhs_nama` akan meningkatkan performance

## 🚀 Ready to Use

Endpoints sudah siap digunakan tanpa perlu stored procedure tambahan. Frontend bisa langsung menggunakan untuk membuat dropdown yang dinamis dan responsive.