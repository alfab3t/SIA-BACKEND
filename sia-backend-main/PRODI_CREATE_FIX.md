# Fix Create Draft Cuti Akademik by Prodi

## 🚨 Masalah yang Terjadi

**Context:**
- User login sebagai prodi
- Mencoba create draft cuti akademik untuk mahasiswa
- Kemungkinan error primary key violation atau SP tidak berfungsi

**Root Cause:**
1. Method `CreateDraftByProdiAsync` tidak menggunakan SP yang benar
2. Seharusnya menggunakan `sia_createCutiAkademikByProdi` bukan `sia_createCutiAkademik`
3. SP prodi memiliki logik khusus untuk generate ID dan set approval

## ✅ Solusi yang Diterapkan

### 1. Menggunakan SP Khusus Prodi
```csharp
// Gunakan SP khusus untuk prodi: sia_createCutiAkademikByProdi
var cmd = new SqlCommand("sia_createCutiAkademikByProdi", conn)
{
    CommandType = CommandType.StoredProcedure
};

cmd.Parameters.AddWithValue("@p1", "STEP1");
cmd.Parameters.AddWithValue("@p2", dto.TahunAjaran ?? "");
cmd.Parameters.AddWithValue("@p3", dto.Semester ?? "");
cmd.Parameters.AddWithValue("@p4", dto.LampiranSuratPengajuan ?? "");
cmd.Parameters.AddWithValue("@p5", dto.Lampiran ?? "");
cmd.Parameters.AddWithValue("@p6", dto.MhsId ?? "");
cmd.Parameters.AddWithValue("@p7", dto.Menimbang ?? "");
cmd.Parameters.AddWithValue("@p8", dto.ApprovalProdi ?? "");
```

### 2. Fallback Mechanism
```csharp
try
{
    // Coba gunakan SP prodi
    await cmd.ExecuteNonQueryAsync();
    // Ambil draft ID yang dibuat SP
    return draftId?.ToString();
}
catch (SqlException ex) when (ex.Number == 2627) // Primary key violation
{
    // Jika SP gagal, gunakan direct insert dengan unique ID
    return await CreateDraftByProdiDirectAsync(dto, conn);
}
```

### 3. Enhanced Error Handling
```csharp
try
{
    Console.WriteLine($"CreateDraftByProdi - MhsId: {dto.MhsId}, ApprovalProdi: {dto.ApprovalProdi}");
    
    var id = await _service.CreateDraftByProdiAsync(dto);
    
    if (string.IsNullOrEmpty(id))
    {
        return BadRequest(new { message = "Gagal membuat draft cuti akademik." });
    }
    
    return Ok(new { draftId = id });
}
catch (Exception ex)
{
    return BadRequest(new { 
        message = "Terjadi kesalahan saat membuat draft.", 
        error = ex.Message 
    });
}
```

## 🔧 Perbaikan yang Dilakukan

### 1. Repository Layer

#### CreateDraftByProdiAsync
- ✅ **Primary Method**: Menggunakan SP `sia_createCutiAkademikByProdi`
- ✅ **Fallback Method**: Direct insert jika SP gagal
- ✅ **Retry Mechanism**: Handle primary key collision
- ✅ **Proper Parameter Mapping**: Sesuai dengan SP prodi

#### GenerateIdByProdiAsync
- ✅ **Correct SP**: Menggunakan `sia_createCutiAkademikByProdi` untuk STEP2
- ✅ **Proper Query**: Ambil final ID berdasarkan `cak_modif_date`

### 2. Controller Layer
- ✅ **Enhanced Error Handling**: Detailed error messages
- ✅ **Debug Logging**: Console logging untuk troubleshooting
- ✅ **Swagger Documentation**: API documentation yang lengkap
- ✅ **Validation**: Check null/empty results

### 3. DTO Structure
```csharp
public class CreateCutiProdiRequest
{
    public string TahunAjaran { get; set; } = "";
    public string Semester { get; set; } = "";
    public string LampiranSuratPengajuan { get; set; } = "";
    public string Lampiran { get; set; } = "";
    public string MhsId { get; set; } = "";           // NIM mahasiswa
    public string Menimbang { get; set; } = "";       // Alasan cuti
    public string ApprovalProdi { get; set; } = "";   // Username prodi
}
```

## 📋 Stored Procedure Logic

### sia_createCutiAkademikByProdi - STEP1
```sql
-- Generate draft ID otomatis
select @tempIdDraft = (select top 1 cak_id from sia_mscutiakademik where cak_id not like '%CA%' order by cak_id desc);
if @tempIdDraft is null
    select @tempIdDraft = 0;
select @tempIdDraft = @tempIdDraft + 1;

-- Insert dengan status Draft dan approval prodi sudah set
insert into sia_mscutiakademik (
    cak_id, mhs_id, cak_tahunajaran, cak_semester, 
    cak_lampiran_suratpengajuan, cak_lampiran, cak_menimbang, 
    cak_status, cak_approval_prodi, cak_app_prodi_date, 
    cak_created_by, cak_created_date
) values (
    CAST(@tempIdDraft as varchar), @p6, @p2, @p3, @p4, @p5, @p7, 
    'Draft', @p8, GETDATE(), @p8, GETDATE()
);
```

### sia_createCutiAkademikByProdi - STEP2
```sql
-- Generate final ID dengan format PMA
declare @newkey varchar(26)
declare @kode varchar(23) = '/PMA/CA/' + dbo.fnConvertIntToRoman(MONTH(GETDATE())) + '/' + cast(YEAR(GETDATE()) as varchar)

-- Logic untuk generate nomor urut
-- Update draft menjadi final ID dengan status 'Belum Disetujui Wadir 1'
update sia_mscutiakademik set
    cak_id = @newkey,
    cak_status = 'Belum Disetujui Wadir 1',
    cak_modif_by = @p3,
    cak_modif_date = GETDATE()
where cak_id = @p2
```

## 🧪 Testing Scenarios

### Test Case 1: Create Draft by Prodi
```bash
POST /api/CutiAkademik/prodi/draft
Content-Type: multipart/form-data

{
  "mhsId": "0420240032",
  "tahunAjaran": "2024/2025",
  "semester": "Genap",
  "menimbang": "Alasan kesehatan",
  "approvalProdi": "andreas_e",
  "lampiranSuratPengajuan": "file1.pdf",
  "lampiran": "file2.pdf"
}
```

**Expected Response:**
```json
{
  "draftId": "15"
}
```

### Test Case 2: Generate Final ID
```bash
PUT /api/CutiAkademik/prodi/generate-id
Content-Type: application/json

{
  "draftId": "15",
  "modifiedBy": "andreas_e"
}
```

**Expected Response:**
```json
{
  "finalId": "012/PMA/CA/XII/2025"
}
```

## 🚀 Cara Testing

### 1. Via Swagger (Prodi)
1. Restart aplikasi: `dotnet run`
2. Buka: `http://localhost:5234/swagger`
3. Test `POST /api/CutiAkademik/prodi/draft`
4. Isi semua field yang required
5. Execute dan cek response

### 2. Via Frontend (Prodi Login)
1. Login sebagai prodi
2. Buka halaman create cuti akademik
3. Isi data mahasiswa dan alasan
4. Submit form
5. ✅ **Expected**: Berhasil create draft

### 3. Check Console Logs
```
CreateDraftByProdi - MhsId: 0420240032, ApprovalProdi: andreas_e
Draft created successfully with ID: 15
```

## 📝 Troubleshooting

### Jika Error Primary Key Violation
1. **Check Console**: Lihat error message di console
2. **Fallback Active**: Method akan otomatis gunakan direct insert
3. **Unique ID**: Fallback menggunakan timestamp-based ID

### Jika SP Error
1. **Check SP Exists**: Pastikan `sia_createCutiAkademikByProdi` ada di database
2. **Check Parameters**: Pastikan semua parameter sesuai
3. **Check Permissions**: Pastikan user database bisa execute SP

### Jika Draft ID Null
1. **Check MhsId**: Pastikan NIM mahasiswa valid
2. **Check ApprovalProdi**: Pastikan username prodi benar
3. **Check Database**: Pastikan data ter-insert dengan benar

## 🎯 Keuntungan Solusi

1. **SP Compatibility**: Menggunakan SP yang sudah ada dan teruji
2. **Fallback Mechanism**: Jika SP gagal, ada backup method
3. **Auto Approval**: Prodi approval sudah ter-set otomatis
4. **Proper ID Generation**: SP handle ID generation dengan benar
5. **Error Resilience**: Handle berbagai skenario error

## 📊 Flow Prodi vs Mahasiswa

### Mahasiswa Flow
1. **Create Draft** → Direct insert dengan timestamp ID
2. **Update Draft** → Direct SQL update
3. **Generate Final** → SP `sia_createCutiAkademik` STEP2

### Prodi Flow  
1. **Create Draft** → SP `sia_createCutiAkademikByProdi` STEP1
2. **Update Draft** → Hybrid (direct SQL untuk draft, SP untuk final)
3. **Generate Final** → SP `sia_createCutiAkademikByProdi` STEP2

Sekarang prodi bisa create draft cuti akademik untuk mahasiswa dengan lancar!