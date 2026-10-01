# Solution: Backend Tidak Update Status

## Problem Confirmed
- ✅ SQL langsung: Status berubah
- ❌ Backend API: Status tidak berubah

## Root Cause
Backend kemungkinan besar connect ke **database yang berbeda**.

## Langkah Penyelesaian

### 1. Cek Connection String di Backend

#### A. Cek appsettings.json
```bash
# Buka file
sia-backend-main/appsettings.json
```

Cari bagian `ConnectionStrings`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "encrypted_string_here"
  }
}
```

#### B. Cek Environment Variable
Backend menggunakan encryption key dari environment variable:

**Windows (CMD):**
```cmd
echo %DECRYPT_KEY_CONNECTION_STRING%
```

**Windows (PowerShell):**
```powershell
$env:DECRYPT_KEY_CONNECTION_STRING
```

**Linux/Mac:**
```bash
echo $DECRYPT_KEY_CONNECTION_STRING
```

### 2. Decrypt Connection String

Tambahkan logging di `Program.cs` untuk melihat connection string yang digunakan:

```csharp
// Di Program.cs, setelah build configuration
var connString = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    Environment.GetEnvironmentVariable("DECRYPT_KEY_CONNECTION_STRING")
);

// Log connection string (HATI-HATI: Jangan commit ini ke git!)
Console.WriteLine("========================================");
Console.WriteLine("CONNECTION STRING INFO:");
Console.WriteLine($"Server: {new System.Data.SqlClient.SqlConnectionStringBuilder(connString).DataSource}");
Console.WriteLine($"Database: {new System.Data.SqlClient.SqlConnectionStringBuilder(connString).InitialCatalog}");
Console.WriteLine($"User: {new System.Data.SqlClient.SqlConnectionStringBuilder(connString).UserID}");
Console.WriteLine("========================================");
```

### 3. Bandingkan dengan SQL Management Studio

Di SQL Management Studio, cek database yang kamu gunakan:

```sql
SELECT 
    DB_NAME() AS 'Current Database',
    @@SERVERNAME AS 'Server Name',
    SUSER_NAME() AS 'Login Name'
```

**Expected Result:**
```
Current Database: ERP_PolmanAstra_NDA
Server Name: [nama_server]
Login Name: [nama_user]
```

### 4. Pastikan Backend Connect ke Database yang Sama

Backend harus connect ke:
- **Server:** [sama dengan SQL Management Studio]
- **Database:** `ERP_PolmanAstra_NDA`
- **User:** [user yang punya permission]

### 5. Test dengan Logging yang Sudah Ditambahkan

Setelah update repository dengan logging detail, jalankan backend dan cek log:

```
========================================
DEBUG GetIdByDraftAsync - START
Input ID: '16/PMA/DO/I/2026'
Connection String (masked): Data Source=...
Connection opened to: ERP_PolmanAstra_NDA on SERVER_NAME  <-- CEK INI!
BEFORE SP - Status: [status_lama]
AFTER SP - Status: Belum Disetujui Wadir 1  <-- HARUS BERUBAH!
========================================
```

**Yang perlu dicek:**
- Database name: Harus `ERP_PolmanAstra_NDA`
- Server name: Harus sama dengan SQL Management Studio
- AFTER SP: Status harus berubah

### 6. Kemungkinan Skenario

#### Skenario A: Database Berbeda
Backend connect ke database lain (misal: `ERP_PolmanAstra_NDA_DEV` atau `ERP_PolmanAstra_NDA_TEST`)

**Solusi:**
- Update connection string di `appsettings.json`
- Atau set environment variable yang benar

#### Skenario B: Server Berbeda
Backend connect ke server lain (misal: development server vs production server)

**Solusi:**
- Update connection string untuk point ke server yang benar

#### Skenario C: User Permission
User backend tidak punya permission untuk UPDATE

**Solusi:**
```sql
-- Grant permission ke user backend
GRANT UPDATE ON sia_msdropout TO [backend_user]
GRANT EXECUTE ON sia_getIdDOByDraft TO [backend_user]
```

### 7. Quick Test

Tambahkan endpoint debug untuk cek database:

```csharp
[HttpGet("debug/database-info")]
public async Task<IActionResult> GetDatabaseInfo()
{
    await using var conn = new SqlConnection(_conn);
    await conn.OpenAsync();
    
    var cmd = new SqlCommand(@"
        SELECT 
            DB_NAME() AS DatabaseName,
            @@SERVERNAME AS ServerName,
            SUSER_NAME() AS LoginName,
            USER_NAME() AS UserName
    ", conn);
    
    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return Ok(new {
            database = reader["DatabaseName"],
            server = reader["ServerName"],
            login = reader["LoginName"],
            user = reader["UserName"]
        });
    }
    
    return Ok(new { message = "Unable to get database info" });
}
```

**Test endpoint:**
```bash
curl http://localhost:5000/api/DropOut/debug/database-info \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:**
```json
{
  "database": "ERP_PolmanAstra_NDA",
  "server": "YOUR_SERVER_NAME",
  "login": "backend_user",
  "user": "dbo"
}
```

### 8. Jika Database Sudah Benar

Jika database sudah benar tapi masih tidak update, kemungkinan:

#### A. Transaction Isolation Level
Backend mungkin read dari snapshot yang lama.

**Solusi:** Tambahkan di repository:
```csharp
// Sebelum execute SP
await using var transaction = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);
cmd.Transaction = transaction;

// Execute SP
// ...

// Commit
await transaction.CommitAsync();
```

#### B. Cache
Ada caching layer yang return data lama.

**Solusi:**
- Restart backend
- Clear cache jika ada
- Disable caching sementara untuk testing

#### C. Multiple Backend Instances
Ada multiple instance backend yang running, dan kamu test ke instance yang berbeda.

**Solusi:**
- Stop semua backend instances
- Start hanya 1 instance
- Test lagi

### 9. Checklist Debugging

- [ ] Cek connection string di appsettings.json
- [ ] Cek environment variable DECRYPT_KEY_CONNECTION_STRING
- [ ] Tambahkan logging di Program.cs untuk decrypt connection string
- [ ] Jalankan backend dan cek log database name
- [ ] Bandingkan dengan SQL Management Studio
- [ ] Test endpoint debug/database-info
- [ ] Cek permission user backend
- [ ] Restart backend dan test lagi
- [ ] Pastikan hanya 1 backend instance yang running

### 10. Expected Behavior Setelah Fix

1. **Backend log menunjukkan:**
   ```
   Connection opened to: ERP_PolmanAstra_NDA on [server_name]
   BEFORE SP - Status: [status_lama]
   AFTER SP - Status: Belum Disetujui Wadir 1
   ```

2. **Database query menunjukkan:**
   ```sql
   SELECT dro_status FROM sia_msdropout WHERE dro_id = '16/PMA/DO/I/2026'
   -- Result: Belum Disetujui Wadir 1
   ```

3. **API response:**
   ```json
   {
     "message": "ID DO berhasil di-generate",
     "oldId": "16/PMA/DO/I/2026",
     "newId": "16/PMA/DO/I/2026"
   }
   ```

## Summary

Masalah paling umum: **Backend connect ke database yang berbeda**.

Langkah paling penting:
1. Cek connection string di backend
2. Bandingkan dengan database yang kamu test di SQL
3. Pastikan server dan database name sama

Setelah itu, backend akan update status dengan benar seperti di SQL.
