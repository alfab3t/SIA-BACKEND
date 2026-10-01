# Debug Guide: Status Tidak Berubah di Backend

## Problem
- SQL langsung: Status berubah ✅
- Backend API: Status tidak berubah ❌

## Langkah Debugging

### 1. Jalankan SQL Debug Script

```sql
-- Run file: SQL_DEBUG_RESUBMIT.sql
-- Script ini akan mengecek:
-- - Data sebelum dan sesudah SP
-- - Trigger yang mungkin mencegah update
-- - Permission
-- - Database yang digunakan
-- - Transaction settings
```

**Yang perlu dicek:**
- Apakah SP benar-benar mengupdate data?
- Apakah ada trigger yang rollback perubahan?
- Apakah ada multiple database dengan nama mirip?

### 2. Cek Backend Logs

Setelah update repository, backend akan menampilkan log detail:

```
========================================
DEBUG GetIdByDraftAsync - START
Input ID: '16/PMA/DO/I/2026'
Connection String (masked): Data Source=...
Parameter added: @dro_id_draft = '16/PMA/DO/I/2026'
Connection opened to: ERP_PolmanAstra_NDA on SERVER_NAME
Executing SP: sia_getIdDOByDraft
BEFORE SP - Status: Ditolak, Created: 2026-01-15 10:30:00
SP executed, HasRows: True
SP returned ID: '16/PMA/DO/I/2026'
AFTER SP - Status: Belum Disetujui Wadir 1, Created: 2026-03-06 12:00:00
DEBUG GetIdByDraftAsync - END (SUCCESS)
========================================
```

**Yang perlu dicek:**
- Database name: Apakah benar `ERP_PolmanAstra_NDA`?
- Server name: Apakah sama dengan SQL Management Studio?
- BEFORE vs AFTER: Apakah status berubah?

### 3. Kemungkinan Penyebab

#### A. Database Berbeda
Backend mungkin connect ke database yang berbeda.

**Cek:**
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

**Solusi:**
- Pastikan connection string mengarah ke database yang sama
- Cek server name dan database name

#### B. Transaction Tidak Di-Commit
SP mungkin dalam transaction yang tidak di-commit.

**Cek SP:**
```sql
-- Pastikan tidak ada BEGIN TRANSACTION tanpa COMMIT
-- Atau tambahkan explicit commit
ALTER PROCEDURE sia_getIdDOByDraft
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ... kode SP ...
    
    -- Tidak perlu COMMIT jika tidak ada BEGIN TRANSACTION
END
```

**Note:** Stored procedure secara default auto-commit kecuali ada explicit transaction.

#### C. Trigger yang Rollback
Ada trigger yang mencegah update.

**Cek:**
```sql
-- Lihat hasil dari SQL_DEBUG_RESUBMIT.sql bagian 4
-- Jika ada trigger, cek definisinya
SELECT OBJECT_DEFINITION(OBJECT_ID('trigger_name'))
```

#### D. Permission Issue
User backend tidak punya permission untuk update.

**Cek:**
```sql
-- Lihat hasil dari SQL_DEBUG_RESUBMIT.sql bagian 5
-- Pastikan Has UPDATE Permission = 1
```

#### E. Cache di Backend
Backend mungkin return cached data.

**Solusi:**
- Restart backend
- Clear cache jika ada
- Cek apakah ada caching middleware

#### F. Frontend Cache
Frontend mungkin menampilkan cached data.

**Solusi:**
- Hard refresh browser (Ctrl+F5)
- Clear browser cache
- Cek Network tab di DevTools

### 4. Test Step by Step

#### Test 1: Cek Connection String
```bash
# Di terminal backend
echo $env:DECRYPT_KEY_CONNECTION_STRING
```

Atau tambahkan log di Program.cs:
```csharp
var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
);
Console.WriteLine($"Connection String: {connString}");
```

#### Test 2: Test API dengan Postman/cURL

```bash
# Test dengan cURL
curl -X PUT "http://localhost:5000/api/DropOut/draft/generate-id?id=16/PMA/DO/I/2026" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -v
```

**Cek response:**
```json
{
  "message": "ID DO berhasil di-generate",
  "oldId": "16/PMA/DO/I/2026",
  "newId": "16/PMA/DO/I/2026"
}
```

#### Test 3: Cek Database Langsung Setelah API Call

```sql
-- Jalankan SEGERA setelah API call
SELECT 
    dro_id,
    dro_status,
    dro_created_date,
    dro_updated_date
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'
```

**Expected:** Status harus berubah ke "Belum Disetujui Wadir 1"

#### Test 4: Cek dengan SQL Profiler

Gunakan SQL Server Profiler untuk melihat query yang dijalankan backend:

1. Buka SQL Server Profiler
2. Start New Trace
3. Filter by Application Name (biasanya ".Net SqlClient Data Provider")
4. Jalankan API call dari backend
5. Lihat query yang dieksekusi

**Yang perlu dicek:**
- Apakah SP benar-benar dipanggil?
- Apakah ada error?
- Apakah ada rollback?

### 5. Quick Fix Attempts

#### Fix 1: Tambahkan Explicit Transaction di SP

```sql
ALTER PROCEDURE [dbo].[sia_getIdDOByDraft]
    @dro_id_draft VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Update tanggal created dan status
        UPDATE sia_msdropout
        SET dro_created_date = GETDATE(),
            dro_status = 'Belum Disetujui Wadir 1'
        WHERE dro_id = @dro_id_draft;
        
        -- ... rest of SP code ...
        
        COMMIT TRANSACTION;
        
        -- Return result
        SELECT TOP 1 dro_id FROM sia_msdropout 
        WHERE dro_id = @dro_id_draft
        ORDER BY dro_created_date DESC;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
```

#### Fix 2: Tambahkan NOLOCK Hint di Backend

```csharp
// Di repository, tambahkan NOLOCK untuk read
using (var checkCmd = new SqlCommand(
    "SELECT dro_id, dro_status, dro_created_date FROM sia_msdropout WITH (NOLOCK) WHERE dro_id = @id", 
    conn))
{
    // ...
}
```

#### Fix 3: Refresh Data Setelah SP Call

```csharp
// Setelah execute SP, query ulang untuk memastikan
var refreshCmd = new SqlCommand(
    "SELECT dro_status FROM sia_msdropout WHERE dro_id = @id",
    conn
);
refreshCmd.Parameters.AddWithValue("@id", newId);
var currentStatus = await refreshCmd.ExecuteScalarAsync();
Console.WriteLine($"Current status after SP: {currentStatus}");
```

### 6. Checklist Debugging

- [ ] Jalankan SQL_DEBUG_RESUBMIT.sql
- [ ] Cek backend logs untuk database name
- [ ] Cek connection string di appsettings.json
- [ ] Test dengan Postman/cURL
- [ ] Cek database langsung setelah API call
- [ ] Gunakan SQL Profiler untuk trace query
- [ ] Cek apakah ada trigger
- [ ] Cek permission user backend
- [ ] Restart backend dan test lagi
- [ ] Clear browser cache dan test lagi

### 7. Expected Behavior

**Setelah API call berhasil:**

1. Backend log menunjukkan:
   ```
   BEFORE SP - Status: [status_lama]
   AFTER SP - Status: Belum Disetujui Wadir 1
   ```

2. Database query menunjukkan:
   ```sql
   dro_status = 'Belum Disetujui Wadir 1'
   dro_created_date = [tanggal_sekarang]
   ```

3. API response:
   ```json
   {
     "message": "ID DO berhasil di-generate",
     "oldId": "16/PMA/DO/I/2026",
     "newId": "16/PMA/DO/I/2026"
   }
   ```

### 8. Jika Masih Tidak Berhasil

Tambahkan endpoint debug khusus:

```csharp
[HttpGet("debug/check-status/{id}")]
public async Task<IActionResult> DebugCheckStatus(string id)
{
    await using var conn = new SqlConnection(_conn);
    await conn.OpenAsync();
    
    var cmd = new SqlCommand(
        "SELECT dro_id, dro_status, dro_created_date, DB_NAME() as DbName, @@SERVERNAME as ServerName FROM sia_msdropout WHERE dro_id = @id",
        conn
    );
    cmd.Parameters.AddWithValue("@id", id);
    
    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return Ok(new {
            id = reader["dro_id"],
            status = reader["dro_status"],
            createdDate = reader["dro_created_date"],
            database = reader["DbName"],
            server = reader["ServerName"]
        });
    }
    
    return NotFound();
}
```

Test endpoint ini sebelum dan sesudah API call untuk memastikan data benar-benar berubah.

## Summary

Masalah paling umum:
1. **Database berbeda** - Backend connect ke DB lain
2. **Cache** - Browser atau backend cache data lama
3. **Transaction** - SP dalam transaction yang tidak di-commit
4. **Trigger** - Ada trigger yang rollback perubahan

Jalankan SQL_DEBUG_RESUBMIT.sql dan cek backend logs untuk menemukan root cause.
