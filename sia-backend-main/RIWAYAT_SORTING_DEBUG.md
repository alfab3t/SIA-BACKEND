# Debug Guide: Riwayat Sorting Issue

## Problem
Riwayat data tidak menampilkan data terbaru terlebih dahulu (newest first).

## Root Cause Analysis

### 1. Check if SP was executed
SQL script `SQL_FIX_SP_GETDATARIWAYATDO_ROWNUM.sql` sudah dibuat dengan sorting yang benar:
- Default sorting: `a.dro_created_date DESC` (newest first)
- Berbeda dengan Pending yang punya status priority

**ACTION**: Pastikan script sudah di-execute di database!

```sql
-- Run this to verify SP definition
SELECT OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO'))
```

### 2. Backend Configuration
Controller endpoint sudah benar:
- File: `Controllers/DropOutController.cs`
- Line 322-342
- Default sortBy: `"a.dro_created_date desc"`

Repository sudah benar:
- File: `Repositories/Implementations/DropOutRepository.cs`
- Method: `GetRiwayatPaginatedAsync`
- Passes sortBy to SP correctly

### 3. Frontend Check
Frontend mungkin mengirim sortBy value yang berbeda. Check:
- Network tab di browser
- Request payload ke `/api/DropOut/riwayat`
- Parameter `sortBy` yang dikirim

## Testing Steps

### Step 1: Execute SQL Script
```sql
-- Run the update script
USE [ERP_PolmanAstra_NDA]
GO

-- Execute the ALTER PROCEDURE from SQL_FIX_SP_GETDATARIWAYATDO_ROWNUM.sql
-- (Copy paste the entire script)
```

### Step 2: Test SP Directly
```sql
-- Test dengan sortBy kosong (should use default)
EXEC sia_getDataRiwayatDO
    @username = '',
    @keyword = '',
    @sort_by = '',  -- Empty = default to newest first
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;
```

Expected result: Data sorted by `dro_created_date DESC` (newest first)

### Step 3: Test Backend Endpoint
```bash
# Test via API
curl -X GET "http://localhost:5000/api/DropOut/riwayat?username=test&page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

Check console logs:
```
DEBUG GetRiwayatPaginatedAsync - page: 1, pageSize: 10, status: ''
```

### Step 4: Check Frontend Request
Open browser DevTools > Network tab:
- Find request to `/api/DropOut/riwayat`
- Check Query Parameters
- Look for `sortBy` parameter value

## Expected Behavior

### Riwayat (History)
- Default sorting: Date DESC (newest first)
- NO status priority (berbeda dengan Pending)
- Format: `a.dro_created_date DESC`

### Pending (Active)
- Default sorting: Status priority + Date DESC
- Draft & Revisi di atas
- Format: `CASE WHEN a.dro_status IN ('Draft', 'Revisi') THEN 0 ELSE 1 END, a.dro_created_date DESC`

## Quick Fix Checklist

- [ ] Execute `SQL_FIX_SP_GETDATARIWAYATDO_ROWNUM.sql` in database
- [ ] Run `SQL_TEST_RIWAYAT_SORTING.sql` to verify
- [ ] Check backend logs for sortBy value
- [ ] Check frontend network request for sortBy parameter
- [ ] Verify data in database has recent dates

## Common Issues

### Issue 1: SP Not Updated
**Symptom**: Still using old OFFSET/FETCH pagination
**Solution**: Execute the ALTER PROCEDURE script

### Issue 2: Frontend Sending Wrong sortBy
**Symptom**: Backend receives sortBy like "dro_id asc" or old format
**Solution**: Update frontend to send empty string or "a.dro_created_date desc"

### Issue 3: No Recent Data
**Symptom**: All data is old
**Solution**: Check if there's actually recent data in database:
```sql
SELECT TOP 5 dro_id, dro_created_date, dro_status
FROM sia_msdropout
ORDER BY dro_created_date DESC;
```

## Files Modified

1. `SQL_FIX_SP_GETDATARIWAYATDO_ROWNUM.sql` - SP update with ROW_NUMBER
2. `Repositories/Implementations/DropOutRepository.cs` - GetRiwayatPaginatedAsync method
3. `Controllers/DropOutController.cs` - GetRiwayat endpoint (already correct)

## Next Steps

1. Execute SQL script in database
2. Run test script to verify
3. Check frontend sortBy parameter
4. If still not working, check database data itself
