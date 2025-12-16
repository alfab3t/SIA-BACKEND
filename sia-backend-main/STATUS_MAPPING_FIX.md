# Status Parameter Mapping Fix

## Problem
The API was receiving status parameter in lowercase (e.g., `status=disetujui`) but the stored procedures expected exact case-sensitive matches like `"Disetujui"`. This caused the filtering to not work properly.

## Root Cause
- API call: `http://localhost:5234/api/CutiAkademik?mhsId=%25&status=disetujui&role=ROL21`
- Stored procedure `sia_getDataCutiAkademik` expects: `"Disetujui"` (capital D)
- Stored procedure `sia_getDataRiwayatCutiAkademik` expects specific status values

## Solution
Added status normalization in `CutiAkademikService.cs`:

### Status Mapping
| Frontend Input | Normalized Output |
|---------------|-------------------|
| `disetujui` | `Disetujui` |
| `belum disetujui prodi` | `Belum Disetujui Prodi` |
| `belum disetujui wadir 1` | `Belum Disetujui Wadir 1` |
| `menunggu upload sk` | `Menunggu Upload SK` |
| `belum disetujui finance` | `Belum Disetujui Finance` |
| `draft` | `Draft` |
| `dihapus` | `Dihapus` |

### Changes Made
1. **CutiAkademikService.cs**: Added `NormalizeStatus()` method
2. **GetAllAsync()**: Now normalizes status before calling repository
3. **GetRiwayatAsync()**: Now normalizes status before calling repository
4. **Fixed UpdateDraft logic**: Corrected the success/failure condition

## Expected Behavior
Now when you call:
```
GET /api/CutiAkademik?mhsId=%25&status=disetujui&role=ROL21
```

The service will:
1. Receive `status=disetujui`
2. Normalize it to `status=Disetujui`
3. Pass the normalized status to the stored procedure
4. Return the correctly filtered results

## Testing
Test with these URLs:
- `?status=disetujui` → should return approved records
- `?status=belum disetujui prodi` → should return records pending prodi approval
- `?status=menunggu upload sk` → should return records waiting for SK upload