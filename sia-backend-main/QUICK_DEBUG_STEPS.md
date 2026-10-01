# Quick Debug Steps: Status Tidak Berubah di Backend

## Problem
✅ SQL langsung: Status berubah  
❌ Backend API: Status tidak berubah

## Root Cause
Backend kemungkinan connect ke **database yang berbeda**.

## Quick Steps (5 Menit)

### Step 1: Cek Database Backend (1 menit)

**Test endpoint debug:**
```bash
curl http://localhost:5000/api/DropOut/debug/database-info \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:**
```json
{
  "database": "ERP_PolmanAstra_NDA",
  "server": "YOUR_SERVER",
  "login": "backend_user",
  "user": "dbo",
  "serverTime": "2026-03-06 12:00:00",
  "message": "Backend is connected to this database"
}
```

**✅ Jika database = `ERP_PolmanAstra_NDA`:** Lanjut ke Step 2  
**❌ Jika database berbeda:** Fix connection string di `appsettings.json`

### Step 2: Cek Status Data di Backend (1 menit)

**Test endpoint debug:**
```bash
curl "http://localhost:5000/api/DropOut/debug/check-status/16%2FPMA%2FDO%2FI%2F2026" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response akan menunjukkan:**
```json
{
  "id": "16/PMA/DO/I/2026",
  "status": "Ditolak",  <-- Status saat ini
  "createdDate": "2026-01-15 10:30:00",
  "database": "ERP_PolmanAstra_NDA",
  "server": "YOUR_SERVER",
  "message": "Data found in backend database"
}
```

**Catat status saat ini.**

### Step 3: Panggil API Generate ID (1 menit)

```bash
curl -X PUT "http://localhost:5000/api/DropOut/draft/generate-id?id=16/PMA/DO/I/2026" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

**Expected Response:**
```json
{
  "message": "ID DO berhasil di-generate",
  "oldId": "16/PMA/DO/I/2026",
  "newId": "16/PMA/DO/I/2026"
}
```

### Step 4: Cek Status Lagi (1 menit)

**Test endpoint debug lagi:**
```bash
curl "http://localhost:5000/api/DropOut/debug/check-status/16%2FPMA%2FDO%2FI%2F2026" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:**
```json
{
  "id": "16/PMA/DO/I/2026",
  "status": "Belum Disetujui Wadir 1",  <-- HARUS BERUBAH!
  "createdDate": "2026-03-06 12:00:00",  <-- HARUS UPDATE!
  "database": "ERP_PolmanAstra_NDA",
  "server": "YOUR_SERVER"
}
```

**✅ Jika status berubah:** Problem solved!  
**❌ Jika status tidak berubah:** Lanjut ke Step 5

### Step 5: Cek Backend Logs (1 menit)

Lihat console/terminal backend, cari log seperti ini:

```
========================================
DEBUG GetIdByDraftAsync - START
Input ID: '16/PMA/DO/I/2026'
Connection opened to: ERP_PolmanAstra_NDA on SERVER_NAME
BEFORE SP - Status: Ditolak, Created: 2026-01-15 10:30:00
SP executed, HasRows: True
SP returned ID: '16/PMA/DO/I/2026'
AFTER SP - Status: Belum Disetujui Wadir 1, Created: 2026-03-06 12:00:00
========================================
```

**Cek:**
- Database name: Harus `ERP_PolmanAstra_NDA`
- BEFORE vs AFTER: Status harus berubah

**✅ Jika AFTER menunjukkan status berubah:** Backend sudah update, mungkin ada cache di frontend  
**❌ Jika AFTER tidak berubah:** Ada masalah di SP atau permission

## Common Issues & Quick Fix

### Issue 1: Database Berbeda
**Symptom:** `database` di response bukan `ERP_PolmanAstra_NDA`

**Fix:**
1. Buka `sia-backend-main/appsettings.json`
2. Update connection string
3. Restart backend
4. Test lagi

### Issue 2: Data Tidak Ditemukan
**Symptom:** Response 404 "Data not found"

**Fix:**
- ID salah atau data tidak ada di database backend
- Cek di SQL Management Studio apakah data ada
- Pastikan connect ke database yang sama

### Issue 3: Permission Denied
**Symptom:** Error "Permission denied" atau "Cannot update"

**Fix:**
```sql
-- Grant permission ke user backend
USE [ERP_PolmanAstra_NDA]
GO

GRANT UPDATE ON sia_msdropout TO [backend_user]
GRANT EXECUTE ON sia_getIdDOByDraft TO [backend_user]
GO
```

### Issue 4: Status Tidak Berubah Meskipun Database Benar
**Symptom:** Database benar, tapi status tetap tidak berubah

**Possible Causes:**
1. **Transaction tidak di-commit** - Jalankan `SQL_FIX_SP_GETIDDODRAFT.sql`
2. **Trigger yang rollback** - Cek dengan `SQL_DEBUG_RESUBMIT.sql`
3. **Multiple backend instances** - Stop semua, start 1 saja
4. **Cache** - Restart backend

## Testing dengan Postman

### 1. Get Database Info
```
GET http://localhost:5000/api/DropOut/debug/database-info
Headers:
  Authorization: Bearer {{token}}
```

### 2. Check Status Before
```
GET http://localhost:5000/api/DropOut/debug/check-status/16/PMA/DO/I/2026
Headers:
  Authorization: Bearer {{token}}
```

### 3. Generate ID (Resubmit)
```
PUT http://localhost:5000/api/DropOut/draft/generate-id?id=16/PMA/DO/I/2026
Headers:
  Authorization: Bearer {{token}}
  Content-Type: application/json
```

### 4. Check Status After
```
GET http://localhost:5000/api/DropOut/debug/check-status/16/PMA/DO/I/2026
Headers:
  Authorization: Bearer {{token}}
```

## Expected Results

### Before API Call:
```json
{
  "status": "Ditolak",
  "createdDate": "2026-01-15 10:30:00"
}
```

### After API Call:
```json
{
  "status": "Belum Disetujui Wadir 1",
  "createdDate": "2026-03-06 12:00:00"
}
```

## Summary

1. Test endpoint `debug/database-info` untuk cek database
2. Test endpoint `debug/check-status/{id}` sebelum dan sesudah
3. Cek backend logs untuk detail execution
4. Jika database berbeda, fix connection string
5. Jika database sama tapi tidak update, cek permission atau SP

**Most Common Issue:** Backend connect ke database yang berbeda (DEV vs PROD).
