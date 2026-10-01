# Permission Update - Drop Out Controller

## Status: ✅ COMPLETED

Permission untuk endpoint approve dan reject di DropOutController telah diupdate sesuai dengan permission yang ada di database.

---

## Perubahan

### Before (Salah)
```csharp
[RequiresPermission("drop_out.approve")]
[HttpPut("wadir/approve")]
public async Task<IActionResult> ApproveByWadir(...)

[RequiresPermission("drop_out.approve")]
[HttpPut("wadir/reject")]
public async Task<IActionResult> RejectByWadir(...)
```

### After (Benar)
```csharp
[RequiresPermission("drop_out.approve_reject")]
[HttpPut("wadir/approve")]
public async Task<IActionResult> ApproveByWadir(...)

[RequiresPermission("drop_out.approve_reject")]
[HttpPut("wadir/reject")]
public async Task<IActionResult> RejectByWadir(...)
```

---

## Permission Drop Out

Berdasarkan database SSO, permission untuk Drop Out:

| Permission | Kegunaan | Endpoint |
|------------|----------|----------|
| `drop_out.view` | Lihat data Drop Out | GET endpoints |
| `drop_out.approve_reject` | Approve & Reject Drop Out | PUT /wadir/approve, PUT /wadir/reject |
| `drop_out.export` | Export data Drop Out | GET /riwayat/excel |

---

## Endpoint yang Diupdate

### 1. Approve by Wadir
```
PUT /api/dropout/wadir/approve
Permission: drop_out.approve_reject
```

### 2. Reject by Wadir
```
PUT /api/dropout/wadir/reject
Permission: drop_out.approve_reject
```

---

## Cara Cek Permission di Database

```sql
-- Cek permission Drop Out
SELECT * 
FROM sso_mspermission 
WHERE per_name LIKE '%drop_out%'
ORDER BY per_name;

-- Output:
-- drop_out.approve_reject
-- drop_out.export
-- drop_out.view
```

```sql
-- Cek role yang punya permission approve_reject
SELECT 
    r.rol_name AS Role,
    p.per_name AS Permission
FROM sso_msrolepermission rp
INNER JOIN sso_msrole r ON rp.rol_id = r.rol_id
INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
WHERE p.per_name = 'drop_out.approve_reject'
ORDER BY r.rol_name;
```

---

## Testing

### Test Approve
```bash
PUT /api/dropout/wadir/approve?id=001/PMA/DO/I/2026
Authorization: Bearer {token_wadir}
Content-Type: application/json

{
  "approvedBy": "wadir1"
}

# Expected: 200 OK (jika user punya permission drop_out.approve_reject)
# Expected: 403 Forbidden (jika user tidak punya permission)
```

### Test Reject
```bash
PUT /api/dropout/wadir/reject?id=001/PMA/DO/I/2026
Authorization: Bearer {token_wadir}
Content-Type: application/json

{
  "rejectedBy": "wadir1",
  "reason": "Dokumen tidak lengkap"
}

# Expected: 200 OK (jika user punya permission drop_out.approve_reject)
# Expected: 403 Forbidden (jika user tidak punya permission)
```

---

## Build Status

✅ **Build**: Success (0 errors, 0 warnings)
✅ **Diagnostics**: No issues found
✅ **Permission**: Updated to `drop_out.approve_reject`

---

## Notes

- Permission `drop_out.approve_reject` digunakan untuk approve DAN reject
- User yang bisa approve otomatis bisa reject (dan sebaliknya)
- Biasanya permission ini diberikan ke role **Wadir 1**
- Frontend tidak perlu diubah (API contract tetap sama)

---

**Update Date**: January 28, 2026
**Status**: ✅ COMPLETED
