# Konsentrasi Filter Debug Guide

## Problem
Ketika mengirim ID konsentrasi (misalnya "11") ke parameter `konsentrasi` di endpoint `/api/dropout`, tidak ada output yang dikembalikan.

## Root Cause Analysis

### 1. Current Filter Logic
Di stored procedure `sia_getDataPendingDO`, filter konsentrasi menggunakan:
```sql
WHERE b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
```

### 2. Possible Issues
1. **Data Type Mismatch**: `kon_id` di database mungkin integer tapi parameter dikirim sebagai string
2. **No Data**: ID konsentrasi yang dikirim tidak memiliki data dropout
3. **Permission Filter**: User tidak punya akses ke konsentrasi tersebut karena role/permission
4. **Wrong ID**: ID yang dikirim tidak ada di database

## Debug Steps

### Step 1: Check Available Data
Jalankan script ini untuk cek data yang tersedia:
```sql
-- File: SQL_CHECK_KONSENTRASI_DATA.sql
```

### Step 2: Test Filter Logic
Jalankan script ini untuk test filter:
```sql
-- File: SQL_TEST_KONSENTRASI_ID_ISSUE.sql
```

### Step 3: Apply Robust Fix
Jalankan script ini untuk fix yang lebih robust:
```sql
-- File: SQL_FIX_KONSENTRASI_FILTER_ROBUST.sql
```

## Solutions

### Solution 1: Frontend Fix (Recommended)
Frontend harus mengirim `value` (ID) dari response endpoint `/api/dropout/konsentrasi`, bukan `text` (nama).

**Example:**
```javascript
// WRONG - sending text
konsentrasi: "Teknologi Rekayasa Pemeliharaan Alat Berat"

// CORRECT - sending value (ID)
konsentrasi: "11"
```

### Solution 2: Backend Fix (Alternative)
Update stored procedure untuk support filter berdasarkan nama konsentrasi juga:
```sql
-- Support both ID and name
WHERE (b.kon_id = @kon_id OR c.kon_nama = @kon_id)
```

### Solution 3: Hybrid Approach
Backend bisa detect apakah parameter adalah ID (numeric) atau nama (string) dan filter accordingly.

## Testing

### Test Cases
1. **Test dengan ID valid**: `konsentrasi = "11"`
2. **Test dengan nama valid**: `konsentrasi = "Teknologi Rekayasa Pemeliharaan Alat Berat"`
3. **Test tanpa filter**: `konsentrasi = ""`
4. **Test dengan ID invalid**: `konsentrasi = "999"`

### Expected Results
- ID valid → return filtered data
- Nama valid → return filtered data (after fix)
- Tanpa filter → return all data
- ID invalid → return empty array

## Debug Output

Setelah apply robust fix, stored procedure akan print debug info:
```
DEBUG - Input Parameters:
@username: admin
@kon_id: 11
@role_id: 

DEBUG - User Info:
@str: 1
@kryid: EMP001
@kons: 11

DEBUG - Konsentrasi Filter: AND (b.kon_id = '11' OR c.kon_nama = '11')
```

## Quick Fix Commands

1. **Check data**: Run `SQL_CHECK_KONSENTRASI_DATA.sql`
2. **Apply fix**: Run `SQL_FIX_KONSENTRASI_FILTER_ROBUST.sql`
3. **Test**: Use Swagger to test with valid konsentrasi ID

## Frontend Integration

Endpoint untuk get konsentrasi options:
```
GET /api/dropout/konsentrasi?prodiId=1
```

Response format:
```json
[
  {
    "value": "11",  // Use this for konsentrasi parameter
    "text": "Teknologi Rekayasa Pemeliharaan Alat Berat"
  }
]
```

## Status
- ❌ **Current**: Filter by ID not working
- ⚠️ **In Progress**: Debug scripts created
- ✅ **Target**: Filter works with both ID and name

---

**Created**: January 28, 2026  
**Status**: Debug in progress