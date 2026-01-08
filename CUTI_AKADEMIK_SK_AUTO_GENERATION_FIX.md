# Cuti Akademik - Auto Generate SK Number Fix (Logika Baru)

## Problem
The `upload-sk` endpoint was successfully uploading SK files and updating status to "Disetujui", but the `srt_no` (SK number) field remained empty because the stored procedure `sia_createSKCutiAkademik` doesn't generate SK numbers automatically.

## Solution (Logika Baru)
Implemented new logic to automatically generate SK numbers with the format: `010/PA-WADIR-I/SKC/IX/2026` (updated for year 2026)

### Changes Made

#### 1. Updated `UploadSKAsync` Method
- **File**: `sia-backend-main/Repositories/Implementations/CutiAkademikRepository.cs`
- **New Logic**:
  - Generate SK number automatically before calling stored procedure
  - Use stored procedure `sia_createSKCutiAkademik` for main upload logic
  - Update `srt_no` field separately after SP execution
  - Fallback to direct SQL if SP fails
  - All dates are set to `GETDATE()` (DateTime.Now equivalent)

#### 2. Enhanced `GenerateSKNumberAsync` Helper Method
- **Purpose**: Generate unique SK numbers with proper format for year 2026
- **Format**: `XXX/PA-WADIR-I/SKC/MM/2026`
  - `XXX`: 3-digit sequence number with leading zeros (001, 002, etc.)
  - `MM`: Roman numeral for month (I, II, III, ..., XII)
  - `2026`: Current year (updated)
- **New Features**:
  - **Yearly continuous numbering**: Sequence continues throughout the year (not reset per month)
  - **Better collision detection**: Improved SQL query with ISNUMERIC check
  - **Enhanced error handling**: More detailed logging and fallback mechanisms
  - **Robust parsing**: Better handling of existing SK number formats

#### 3. Updated Controller Response Message
- **File**: `sia-backend-main/Controllers/CutiAkademikController.cs`
- **Change**: Updated success message to indicate SK number is auto-generated

### SK Number Format Examples (2026)
- January 2026: `001/PA-WADIR-I/SKC/I/2026`
- September 2026: `010/PA-WADIR-I/SKC/IX/2026`
- December 2026: `052/PA-WADIR-I/SKC/XII/2026`

### How The New Logic Works

1. **File Upload**: Admin uploads SK file via `/api/CutiAkademik/upload-sk`
2. **Validation**: System checks record exists and status is "Menunggu Upload SK"
3. **File Save**: SK file is saved to server with encrypted filename
4. **SK Number Generation** (New Logic):
   - Get current month/year (2026)
   - Convert month to Roman numerals
   - Find highest existing sequence for entire year 2026
   - Generate next sequence number with leading zeros
   - Check for collisions and increment if needed
5. **Database Update** (Hybrid Approach):
   - **Primary**: Use stored procedure `sia_createSKCutiAkademik` for main logic
   - **Additional**: Update `srt_no` field with generated SK number
   - **Fallback**: Direct SQL update if SP fails
6. **Final Result**:
   - `cak_sk` = uploaded filename
   - `srt_no` = generated SK number (e.g., "010/PA-WADIR-I/SKC/IX/2026")
   - `cak_status` = 'Disetujui'
   - `cak_status_cuti` = 'Cuti'
   - All dates set to current timestamp
   - `mhs_status_kuliah` = 'Cuti' in mahasiswa table

### Key Improvements in New Logic

1. **Yearly Sequence**: SK numbers increment continuously throughout 2026 (001, 002, 003...)
2. **Stored Procedure Integration**: Uses existing SP while adding SK number generation
3. **Better Error Handling**: More robust collision detection and fallback mechanisms
4. **Enhanced Logging**: Detailed console output for debugging
5. **Year Update**: All examples and logic updated for 2026

### Testing
After the fix, when calling the upload-sk endpoint:

```bash
curl -X 'PUT' \
  'http://localhost:5234/api/CutiAkademik/upload-sk' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'Id=031/PMA/CA/I/2026' \
  -F 'FileSK=@file.pdf;type=application/pdf' \
  -F 'UploadBy=user_admin'
```

**Expected Response**:
```json
{
  "message": "SK berhasil diupload dengan nomor SK yang dibuat otomatis. Status cuti akademik telah diubah menjadi 'Disetujui'."
}
```

**Expected Result in GetAll**:
```json
{
  "id": "031/PMA/CA/I/2026",
  "idDisplay": "031/PMA/CA/I/2026",
  "mhsId": "0320250001",
  "namaMahasiswa": "ABIDIN HABSYI",
  "prodi": "Manajemen Informatika",
  "tahunAjaran": "2024/2025",
  "semester": "Genap",
  "approveProdi": "nda_prodi",
  "approveDir1": "tonny.pongoh",
  "tanggal": "05 Jan 2026",
  "suratNo": "010/PA-WADIR-I/SKC/I/2026",  // ← Now automatically generated for 2026!
  "status": "Disetujui"
}
```

### Implementation Flow

```
1. Upload SK File
   ↓
2. Validate Record Status
   ↓
3. Save File to Server
   ↓
4. Generate SK Number (NEW LOGIC)
   - Query highest sequence for 2026
   - Increment sequence
   - Format: XXX/PA-WADIR-I/SKC/MM/2026
   ↓
5. Execute Stored Procedure
   - sia_createSKCutiAkademik
   - Updates status, dates, file path
   ↓
6. Update SK Number
   - SET srt_no = generated number
   - SET cak_modif_date = GETDATE()
   ↓
7. Return Success
```

## Benefits of New Logic
1. **Automatic SK Number Generation**: No manual input required
2. **Consistent Format**: All SK numbers follow the same pattern for 2026
3. **Yearly Continuity**: Sequential numbering throughout the year
4. **Collision Prevention**: System ensures unique SK numbers
5. **SP Integration**: Works with existing stored procedure logic
6. **Robust Fallback**: Direct SQL if SP fails
7. **Enhanced Logging**: Better debugging and monitoring
8. **Date Consistency**: All dates set to current timestamp (2026)

## Files Modified
1. `sia-backend-main/Repositories/Implementations/CutiAkademikRepository.cs`
2. `sia-backend-main/Controllers/CutiAkademikController.cs`

The new logic ensures that every time an SK is uploaded, a unique SK number is automatically generated for year 2026 and stored in the `srt_no` field, while maintaining compatibility with the existing stored procedure workflow.