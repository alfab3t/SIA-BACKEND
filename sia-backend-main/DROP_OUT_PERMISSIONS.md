# Drop Out - Daftar Lengkap Permission

## Summary Permission yang Digunakan

Drop Out Controller menggunakan **6 permission**:

1. ✅ `drop_out.view` - Melihat data (paling banyak digunakan)
2. ✅ `drop_out.create` - Membuat pengajuan baru
3. ✅ `drop_out.edit` - Mengedit data
4. ✅ `drop_out.delete` - Menghapus data
5. ✅ `drop_out.approve_reject` - Approve/Reject pengajuan
6. ✅ `drop_out.import` - Upload file SK/SKPB
7. ✅ `drop_out.export` - Download file SK/SKPB

---

## Detail Permission per Endpoint

### 1. VIEW Permission (`drop_out.view`)

**Total**: 18 endpoints menggunakan permission ini

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/debug-claims` | GET | Debug JWT claims |
| 2 | `/api/dropout` | GET | List semua Drop Out |
| 3 | `/api/dropout/{id}` | GET | Get Drop Out by ID |
| 4 | `/api/dropout/detail` | GET | Get detail lengkap |
| 5 | `/api/dropout/{id}/check-report` | GET | Check report suffix |
| 6 | `/api/dropout/report-suket/{suratNo}` | GET | Get report surat keterangan |
| 7 | `/api/dropout/riwayat` | GET | Get riwayat Drop Out |
| 8 | `/api/dropout/riwayat/excel` | GET | Get riwayat Excel |
| 9 | `/api/dropout/{id}/report-sk` | GET | Get report SK |
| 10 | `/api/dropout/{id}/report-sk-sub` | GET | Get report SK subreport |
| 11 | `/api/dropout/pending` | GET | Get pending list |
| 12 | `/api/dropout/mahasiswa-by-konsentrasi` | GET | Get mahasiswa by konsentrasi |
| 13 | `/api/dropout/prodi` | GET | Get prodi by user |
| 14 | `/api/dropout/prodi/list` | GET | Get all prodi |
| 15 | `/api/dropout/konsentrasi` | GET | Get konsentrasi by prodi |
| 16 | `/api/dropout/mahasiswa` | GET | Get mahasiswa list |
| 17 | `/api/dropout/angkatan-by-mahasiswa` | GET | Get angkatan mahasiswa |
| 18 | `/api/dropout/mahasiswa/{mhsId}/bebas-tanggungan` | GET | Cek bebas tanggungan |
| 19 | `/api/dropout/mahasiswa/{mhsId}/profil` | GET | Get profil mahasiswa |
| 20 | `/api/dropout/template-sk` | GET | Download template SK |
| 21 | `/api/dropout/template-sk/list` | GET | List template SK |

**Kegunaan**: User dengan permission ini bisa **melihat semua data** Drop Out, termasuk list, detail, riwayat, dan data master (prodi, mahasiswa, dll).

---

### 2. CREATE Permission (`drop_out.create`)

**Total**: 1 endpoint

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/create-pengajuan` | POST | Create pengajuan Drop Out baru |

**Kegunaan**: User dengan permission ini bisa **membuat pengajuan Drop Out baru**.

---

### 3. EDIT Permission (`drop_out.edit`)

**Total**: 2 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/{id}` | PUT | Update data Drop Out |
| 2 | `/api/dropout/draft/{id}/generate-id` | PUT | Generate ID dari draft |

**Kegunaan**: User dengan permission ini bisa **mengedit/update data** Drop Out yang sudah ada.

---

### 4. DELETE Permission (`drop_out.delete`)

**Total**: 1 endpoint

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/{id}` | DELETE | Delete Drop Out |

**Kegunaan**: User dengan permission ini bisa **menghapus data** Drop Out.

---

### 5. APPROVE_REJECT Permission (`drop_out.approve_reject`)

**Total**: 2 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/wadir/approve` | PUT | Approve Drop Out by Wadir |
| 2 | `/api/dropout/wadir/reject` | PUT | Reject Drop Out by Wadir |

**Kegunaan**: User dengan permission ini bisa **approve atau reject** pengajuan Drop Out (biasanya Wadir 1).

---

### 6. IMPORT Permission (`drop_out.import`)

**Total**: 2 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/upload-sk` | PUT | Upload SK path (old method) |
| 2 | `/api/dropout/upload-sk-file` | POST | Upload file SK & SKPB |

**Kegunaan**: User dengan permission ini bisa **upload file SK dan SKPB**.

---

### 7. EXPORT Permission (`drop_out.export`)

**Total**: 5 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/dropout/download-sk/{droId}` | GET | Download SK info |
| 2 | `/api/dropout/download-sk-file/{droId}` | GET | Download SK file by ID |
| 3 | `/api/dropout/download-sk-file?id={id}` | GET | Download SK file by query |
| 4 | `/api/dropout/sk/{filename}` | GET | Download SK by filename |
| 5 | `/api/dropout/skpb/{filename}` | GET | Download SKPB by filename |
| 6 | `/api/dropout/download-all-sk` | GET | Download SK + SKPB (ZIP) |

**Kegunaan**: User dengan permission ini bisa **download file SK dan SKPB**.

---

## Rekomendasi Role Permission

### Admin
```
✅ drop_out.view
✅ drop_out.create
✅ drop_out.edit
✅ drop_out.delete
✅ drop_out.approve_reject
✅ drop_out.import
✅ drop_out.export
```
**Total**: 7 permissions (ALL)

### Wadir 1
```
✅ drop_out.view
✅ drop_out.edit
✅ drop_out.approve_reject
✅ drop_out.import
✅ drop_out.export
```
**Total**: 5 permissions

### Staff Akademik
```
✅ drop_out.view
✅ drop_out.create
✅ drop_out.edit
✅ drop_out.import
✅ drop_out.export
```
**Total**: 5 permissions

### Sekprodi
```
✅ drop_out.view
✅ drop_out.create
✅ drop_out.export
```
**Total**: 3 permissions (view, create, download only)

### Mahasiswa
```
✅ drop_out.view (own data only)
✅ drop_out.export (own data only)
```
**Total**: 2 permissions (view & download own data only)

---

## SQL untuk Setup Permission

### 1. Cek Permission yang Ada
```sql
SELECT * FROM sso_mspermission 
WHERE per_name LIKE 'drop_out.%'
ORDER BY per_name
```

### 2. Insert Permission (jika belum ada)
```sql
-- View
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_VIEW', 'drop_out.view', 'View Drop Out', 'Aktif')

-- Create
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_CREATE', 'drop_out.create', 'Create Drop Out', 'Aktif')

-- Edit
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_EDIT', 'drop_out.edit', 'Edit Drop Out', 'Aktif')

-- Delete
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_DELETE', 'drop_out.delete', 'Delete Drop Out', 'Aktif')

-- Approve/Reject
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_APPROVE', 'drop_out.approve_reject', 'Approve/Reject Drop Out', 'Aktif')

-- Import
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_IMPORT', 'drop_out.import', 'Upload SK Drop Out', 'Aktif')

-- Export
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_DO_EXPORT', 'drop_out.export', 'Download SK Drop Out', 'Aktif')
```

### 3. Assign Permission ke Role
```sql
-- Contoh: Assign semua permission ke Admin
INSERT INTO sso_msrolepermission (rol_id, per_id)
SELECT 'ROL_ADMIN', per_id 
FROM sso_mspermission 
WHERE per_name LIKE 'drop_out.%'

-- Contoh: Assign permission ke Wadir 1
INSERT INTO sso_msrolepermission (rol_id, per_id)
SELECT 'ROL_WADIR1', per_id 
FROM sso_mspermission 
WHERE per_name IN (
    'drop_out.view',
    'drop_out.edit',
    'drop_out.approve_reject',
    'drop_out.import',
    'drop_out.export'
)
```

### 4. Cek User Permission
```sql
-- Cek user yang punya permission Drop Out
SELECT 
    u.usr_username,
    r.rol_name,
    p.per_name,
    p.per_description
FROM sso_msuserrole ur
INNER JOIN sso_msuser u ON ur.usr_id = u.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
INNER JOIN sso_msrolepermission rp ON r.rol_id = rp.rol_id
INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
WHERE p.per_name LIKE 'drop_out.%'
ORDER BY u.usr_username, p.per_name
```

---

## Summary

**Total Permission**: 7
- `drop_out.view` - 21 endpoints
- `drop_out.create` - 1 endpoint
- `drop_out.edit` - 2 endpoints
- `drop_out.delete` - 1 endpoint
- `drop_out.approve_reject` - 2 endpoints
- `drop_out.import` - 2 endpoints
- `drop_out.export` - 6 endpoints

**Total Endpoints**: 35 endpoints

**Permission Paling Banyak Digunakan**: `drop_out.view` (21 endpoints)

---

**Created**: January 28, 2026
**Status**: ✅ COMPLETE
