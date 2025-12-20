# Create SK Implementation - Sesuai Stored Procedure

## Overview
Implementasi Create SK dan Upload SK yang telah disesuaikan dengan stored procedure `sia_createSKCutiAkademik` yang diberikan.

## Stored Procedure `sia_createSKCutiAkademik`
```sql
CREATE PROCEDURE [dbo].[sia_createSKCutiAkademik]
@p1 varchar(max), -- cak_id
@p2 varchar(max), -- cak_sk (filename)
@p3 varchar(max), -- cak_modif_by
-- @p4 sampai @p50 (tidak digunakan)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update tabel cuti akademik
    UPDATE sia_mscutiakademik
    SET cak_sk = @p2,
        cak_status = 'Disetujui',
        cak_status_cuti = 'Cuti',
        cak_approval_dakap = GETDATE(),
        cak_modif_by = @p3,
        cak_modif_date = GETDATE()
    WHERE cak_id = @p1
    
    -- Update status mahasiswa
    UPDATE sia_msmahasiswa 
    SET mhs_status_kuliah = 'Cuti' 
    WHERE mhs_id = (SELECT mhs_id FROM sia_mscutiakademik WHERE cak_id = @p1)
END
```

## Implementasi Backend

### 1. Method `CreateSKAsync` ✅ **AKTIF**
**Purpose**: Generate nomor SK dan langsung finalisasi menggunakan SP

**Flow**:
1. Validasi record exists dan status valid
2. Generate nomor SK otomatis jika tidak disediakan
3. **Coba SP dulu**: Panggil `sia_createSKCutiAkademik` dengan nomor SK
4. **Fallback**: Jika SP gagal, update status ke "Menunggu Upload SK"

**Request**:
```json
{
  "id": "038/PMA/CA/XIII/2025",
  "noSK": "001/SK-CA/12/2025",  // Optional - auto-generated
  "createdBy": "admin_user"
}
```

**Response**:
```json
{
  "message": "SK berhasil dibuat dan siap untuk diupload.",
  "noSK": "001/SK-CA/12/2025",
  "status": "Disetujui"
}
```

### 2. Method `UploadSKAsync` ✅ **DIPERBARUI**
**Purpose**: Upload file SK dan finalisasi menggunakan SP

**Flow**:
1. Validasi record exists dan status = "Menunggu Upload SK"
2. Save file ke `wwwroot/uploads/cuti/`
3. **Coba SP dulu**: Panggil `sia_createSKCutiAkademik` dengan filename
4. **Fallback**: Jika SP gagal, update manual dengan semua field

**Request**: `multipart/form-data`
- `id`: ID cuti akademik
- `fileSK`: File SK
- `uploadBy`: Username admin

**Response**:
```json
{
  "message": "SK berhasil diupload. Status cuti akademik telah diubah menjadi 'Disetujui'."
}
```

## Workflow Options

### Option 1: Generate SK + Upload File
1. `POST /api/CutiAkademik/create-sk` - Generate nomor SK
2. `PUT /api/CutiAkademik/upload-sk` - Upload file

### Option 2: Direct Upload (Recommended) ⭐
1. `PUT /api/CutiAkademik/upload-sk` - Langsung upload file dan finalisasi

## Database Changes by SP

### Table `sia_mscutiakademik`:
- ✅ `cak_sk` = filename/nomor SK
- ✅ `cak_status` = 'Disetujui'
- ✅ `cak_status_cuti` = 'Cuti'
- ✅ `cak_approval_dakap` = GETDATE()
- ✅ `cak_modif_by` = user
- ✅ `cak_modif_date` = GETDATE()

### Table `sia_msmahasiswa`:
- ✅ `mhs_status_kuliah` = 'Cuti'

## Hybrid Approach Implementation

### Primary: Stored Procedure
```csharp
var spCmd = new SqlCommand("sia_createSKCutiAkademik", conn)
{
    CommandType = CommandType.StoredProcedure
};

spCmd.Parameters.AddWithValue("@p1", dto.Id);        // cak_id
spCmd.Parameters.AddWithValue("@p2", fileName);      // cak_sk
spCmd.Parameters.AddWithValue("@p3", dto.UploadBy);  // cak_modif_by

// p4-p50 kosong
for (int i = 4; i <= 50; i++)
    spCmd.Parameters.AddWithValue($"@p{i}", "");
```

### Fallback: Direct SQL
```sql
UPDATE sia_mscutiakademik 
SET cak_sk = @fileName,
    cak_status = 'Disetujui',
    cak_status_cuti = 'Cuti',
    cak_approval_dakap = GETDATE(),
    cak_modif_date = GETDATE(),
    cak_modif_by = @uploadBy
WHERE cak_id = @id;

UPDATE sia_msmahasiswa 
SET mhs_status_kuliah = 'Cuti' 
WHERE mhs_id = (SELECT mhs_id FROM sia_mscutiakademik WHERE cak_id = @id);
```

## Status Flow

### Create SK Flow:
1. **"Belum Disetujui Finance"** → Create SK → **"Disetujui"** (jika SP berhasil)
2. **"Belum Disetujui Finance"** → Create SK → **"Menunggu Upload SK"** (jika SP gagal)

### Upload SK Flow:
1. **"Menunggu Upload SK"** → Upload SK → **"Disetujui"**

## Error Handling
- ✅ Comprehensive validation untuk semua input
- ✅ File type dan size validation (untuk upload)
- ✅ Status validation sebelum operasi
- ✅ Detailed logging untuk debugging SP dan fallback
- ✅ Graceful error responses dengan pesan yang jelas
- ✅ Hybrid approach memastikan reliability

## Testing Endpoints

### Test Create SK:
```bash
POST http://localhost:5234/api/CutiAkademik/create-sk
Content-Type: application/json

{
  "id": "038/PMA/CA/XIII/2025",
  "createdBy": "admin_user"
}
```

### Test Upload SK:
```bash
PUT http://localhost:5234/api/CutiAkademik/upload-sk
Content-Type: multipart/form-data

id: 038/PMA/CA/XIII/2025
uploadBy: admin_user
fileSK: [file upload]
```

## Keunggulan Implementasi
1. **SP Compliance**: Sesuai 100% dengan stored procedure yang diberikan
2. **Hybrid Reliability**: SP primary dengan direct SQL fallback
3. **Complete Updates**: Update lengkap ke tabel cuti dan mahasiswa
4. **Auto Generation**: Nomor SK otomatis dengan format `001/SK-CA/12/2025`
5. **File Management**: Upload file terintegrasi dengan SP workflow
6. **Comprehensive Logging**: Detail logging untuk debugging
7. **Flexible Workflow**: 2 opsi workflow sesuai kebutuhan

## Catatan Penting
- ✅ **Endpoint `CreateSK` sudah diaktifkan** - tidak lagi dikomentari
- ✅ Semua interface dan service sudah diupdate
- ✅ SP `sia_createSKCutiAkademik` digunakan sebagai primary method
- ✅ Hybrid approach: SP primary dengan direct SQL fallback
- ✅ File upload handling terintegrasi dengan SP workflow
- ✅ Status mahasiswa otomatis diupdate ke 'Cuti'
- ✅ DTO `CreateSKCutiAkademikRequest` tersedia untuk keperluan khusus