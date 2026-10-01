# Debug Guide: Download All SK Issue - Timestamp Mismatch

## Problem
Endpoint `/api/DropOut/download-all-sk` hanya menampilkan SKPB, padahal di database ada kedua file (SK dan SKPB).

## Root Cause Analysis

### 1. Timestamp Mismatch Issue ⭐ **MAIN ISSUE**
**Problem**: File upload menambahkan timestamp, tapi database menyimpan nama asli
- **Upload File**: `AssessmentAkhir_MI2A.pdf` → `AssessmentAkhir_MI2A_20260330_143022.pdf`
- **Database**: `AssessmentAkhir_MI2A.pdf` (nama asli)
- **Server File**: `AssessmentAkhir_MI2A_20260330_143022.pdf` (dengan timestamp)

**Flow**:
1. User upload file via `/upload-sk-file` → File disimpan dengan timestamp
2. Frontend call `/upload-sk` → Database diupdate dengan nama asli (tanpa timestamp)
3. Download call `/download-all-sk` → Cari file berdasarkan nama di database → File tidak ditemukan

### 2. URL Download Salah
**Problem**: `downloadUrl` menggunakan `/api/DropOut/file/{fileName}` yang tidak ada
**Fix**: Ganti ke endpoint yang benar:
- SK: `/api/DropOut/sk/{fileName}`
- SKPB: `/api/DropOut/skpb/{fileName}`

## Solution Implemented

### 1. Pattern Matching for Timestamp Files
```csharp
// Jika file tidak ada, cari file dengan timestamp (pattern matching)
if (!System.IO.File.Exists(skFullPath))
{
    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(result.Sk);
    var extension = Path.GetExtension(result.Sk);
    
    // Cari file dengan pattern: originalname_timestamp.ext
    var matchingFiles = Directory.GetFiles(skFolder, $"{fileNameWithoutExt}_*{extension}")
                               .OrderByDescending(f => new FileInfo(f).CreationTime)
                               .ToArray();
    
    if (matchingFiles.Length > 0)
    {
        skFullPath = matchingFiles[0]; // Ambil yang terbaru
    }
}
```

### 2. Enhanced Debug Logging
- Console logs untuk trace execution
- Pattern matching logs
- File existence checks

## Debug Steps

### Step 1: Check Database vs Server Files
```sql
-- Check database content
SELECT dro_id, dro_sk, dro_skpb 
FROM sia_msdropout 
WHERE dro_id = '12/PMA/DO/I/2026'
```

Expected result:
- Database: `AssessmentAkhir_MI2A.pdf`
- Server: `AssessmentAkhir_MI2A_20260330_143022.pdf`

### Step 2: Test Pattern Matching
```bash
# Test the fixed endpoint
GET /api/DropOut/download-all-sk?id=12/PMA/DO/I/2026
```

Console logs will show:
```
=== DEBUG DownloadAllSK ===
Processing SK: 'AssessmentAkhir_MI2A.pdf'
SK file not found, searching with timestamp pattern...
Found SK file with timestamp: 'AssessmentAkhir_MI2A_20260330_143022.pdf'
Added SK file: 'AssessmentAkhir_MI2A_20260330_143022.pdf'
```

### Step 3: Verify File Structure
```
wwwroot/uploads/dropout/
├── sk/
│   ├── AssessmentAkhir_MI2A_20260330_143022.pdf  ← Actual file with timestamp
│   └── (not: AssessmentAkhir_MI2A.pdf)           ← What database expects
└── skpb/
    └── Sistem Mutu Astra_20260330_143022.pdf     ← Actual file with timestamp
```

## Expected Response After Fix

### Success Case (Both Files Found)
```json
{
  "message": "File SK dan SKPB tersedia",
  "doId": "12/PMA/DO/I/2026",
  "files": [
    {
      "type": "SK",
      "fileName": "AssessmentAkhir_MI2A_20260330_143022.pdf",
      "downloadUrl": "/api/DropOut/sk/AssessmentAkhir_MI2A_20260330_143022.pdf"
    },
    {
      "type": "SKPB", 
      "fileName": "Sistem Mutu Astra_20260330_143022.pdf",
      "downloadUrl": "/api/DropOut/skpb/Sistem Mutu Astra_20260330_143022.pdf"
    }
  ],
  "debug": {
    "skFromDb": "AssessmentAkhir_MI2A.pdf",
    "skpbFromDb": "Sistem Mutu Astra.pdf"
  }
}
```

## Alternative Solutions

### Option 1: Fix Frontend (Recommended)
Update frontend untuk mengirim nama file dengan timestamp ke database:
```javascript
// Frontend should send the actual filename with timestamp
const uploadData = {
  droId: "12/PMA/DO/I/2026",
  SK: "AssessmentAkhir_MI2A_20260330_143022.pdf",  // With timestamp
  SKPB: "Sistem Mutu Astra_20260330_143022.pdf"   // With timestamp
}
```

### Option 2: Remove Timestamp (Alternative)
Modify upload to not add timestamp:
```csharp
// Remove this line in UploadSKFile
// var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
// var skFileName = $"{originalFileName}_{timestamp}{extension}";

// Use original filename
var skFileName = request.SkFile.FileName;
```

### Option 3: Pattern Matching (Current Implementation)
Keep current fix that searches for files with timestamp pattern.

## Testing Scenarios

### Test 1: Database has original name, server has timestamp
- Database: `file.pdf`
- Server: `file_20260330_143022.pdf`
- Expected: ✅ Found via pattern matching

### Test 2: Database has timestamp, server has timestamp
- Database: `file_20260330_143022.pdf`
- Server: `file_20260330_143022.pdf`
- Expected: ✅ Direct match

### Test 3: Multiple timestamp versions
- Server: `file_20260330_143022.pdf`, `file_20260330_150000.pdf`
- Expected: ✅ Returns newest file

---

**Status**: ✅ FIXED (Pattern Matching Implementation)
**Date**: March 30, 2026
**Issue**: Timestamp mismatch between database and server files