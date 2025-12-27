# API Guide - Pengajuan Meninggal Dunia (Updated)

## Endpoint untuk Form Pengajuan

### 1. Get Data Mahasiswa (Dropdown) - Using Stored Procedure
```
GET /api/MeninggalDunia/mahasiswa-dropdown
```
**Response:**
```json
[
  {
    "value": "032024001",
    "text": "ABDUL AZIZ",
    "nimNama": "032024001 - ABDUL AZIZ"
  }
]
```

### 2. Get Data Mahasiswa (Search) - Using Direct Query
```
GET /api/MeninggalDunia/mahasiswa?search=nama
```
**Response:**
```json
[
  {
    "mhsId": "032024001",
    "mhsNama": "ABDUL AZIZ",
    "mhsAngkatan": "2020",
    "programStudi": "Teknik Informatika",
    "konsentrasi": "Software Engineering"
  }
]
```

### 3. Get Detail Mahasiswa (Auto-fill Program Studi & Angkatan)
```
GET /api/MeninggalDunia/mahasiswa/032024001
```
**Response:**
```json
{
  "mhsId": "032024001",
  "mhsNama": "ABDUL AZIZ",
  "mhsAngkatan": "2020",
  "programStudi": "Teknik Informatika",
  "programStudiSingkatan": "TI",
  "konsentrasi": "Software Engineering",
  "konsentrasiId": "TI001"
}
```

### 4. Get Prodi Mahasiswa (Using Stored Procedure)
```
GET /api/MeninggalDunia/mahasiswa/032024001/prodi
```
**Response:**
```json
{
  "konId": "TI001",
  "proId": "TI",
  "proNama": "Teknik Informatika"
}
```

### 5. Create Pengajuan Meninggal Dunia (Simplified)
```
POST /api/MeninggalDunia
Content-Type: multipart/form-data
```

**Form Data (Hanya 2 Field):**
- `MhsId` (required): "032024001"
- `LampiranFile` (required): File surat kematian

**Response:**
```json
{
  "id": "123" // Draft ID (numeric)
}
```

### 6. Finalize Pengajuan
```
POST /api/MeninggalDunia/finalize/123
```
**Response:**
```json
{
  "officialId": "001/PA/MD/XII/2025"
}
```

## Frontend Flow (Simplified)

### 1. Load Form (Multiple Options):
```javascript
// Option 1: Using Stored Procedure (Recommended)
const mahasiswaDropdown = await fetch('/api/MeninggalDunia/mahasiswa-dropdown');

// Option 2: Using Direct Query with Search
const mahasiswaSearch = await fetch('/api/MeninggalDunia/mahasiswa?search=abdul');
```

### 2. Auto-fill saat pilih mahasiswa (Multiple Options):
```javascript
// Option 1: Get full detail mahasiswa
const selectedMhsId = "032024001";
const mahasiswaDetail = await fetch(`/api/MeninggalDunia/mahasiswa/${selectedMhsId}`);

// Auto-fill form fields
document.getElementById('programStudi').value = mahasiswaDetail.programStudi;
document.getElementById('tahunAngkatan').value = mahasiswaDetail.mhsAngkatan;

// Option 2: Get only prodi info using SP
const prodiInfo = await fetch(`/api/MeninggalDunia/mahasiswa/${selectedMhsId}/prodi`);
document.getElementById('programStudi').value = prodiInfo.proNama;
```

### 3. Submit Form:
```javascript
const formData = new FormData();
formData.append('MhsId', selectedMhsId);
formData.append('LampiranFile', fileInput.files[0]);

const response = await fetch('/api/MeninggalDunia', {
  method: 'POST',
  body: formData
});
```

## Backend Process

### Auto-fill Logic:
1. **Input**: Hanya `MhsId` + `LampiranFile`
2. **Backend**: Otomatis ambil data mahasiswa dari database
3. **Auto-fill**: Program Studi, Angkatan, Konsentrasi
4. **Save**: Simpan dengan data lengkap

### Database Flow:
1. **STEP1**: Create draft dengan data mahasiswa lengkap
2. **STEP2**: Convert ke official ID jika diperlukan

## Keuntungan Perbaikan

✅ **Simplified Form**: Hanya 2 field input (MhsId + File)  
✅ **Auto-fill**: Program Studi & Angkatan otomatis terisi  
✅ **Data Consistency**: Data mahasiswa selalu akurat dari database  
✅ **User Friendly**: Prodi tidak perlu input manual  
✅ **File Upload**: Langsung handle file surat kematian  

## Endpoint Summary

| Method | Endpoint | Purpose | Source |
|--------|----------|---------|---------|
| GET | `/mahasiswa-dropdown` | Dropdown mahasiswa | SP: lpm_getListMahasiswa |
| GET | `/mahasiswa?search=` | Search mahasiswa | Direct Query |
| GET | `/mahasiswa/{id}` | Detail mahasiswa (auto-fill) | Direct Query |
| GET | `/mahasiswa/{id}/prodi` | Prodi mahasiswa | SP: lpm_getListMahasiswaByProdi |
| POST | `/` | Create pengajuan (MhsId + File) | SP: sia_createMeninggalDunia |
| POST | `/finalize/{id}` | Convert draft ke official | SP: sia_createMeninggalDunia |