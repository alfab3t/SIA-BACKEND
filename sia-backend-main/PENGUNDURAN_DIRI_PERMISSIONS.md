# Pengunduran Diri - Daftar Lengkap Permission

## Summary Permission yang Digunakan

Pengunduran Diri Controller menggunakan **6 permission**:

1. ✅ `pengunduran_diri.view` - Melihat data (paling banyak digunakan)
2. ✅ `pengunduran_diri.create` - Membuat pengajuan baru
3. ✅ `pengunduran_diri.edit` - Mengedit data
4. ✅ `pengunduran_diri.delete` - Menghapus data
5. ✅ `pengunduran_diri.approve_reject` - Approve/Reject pengajuan
6. ✅ `pengunduran_diri.import` - Upload file (lampiran, SK, SKPB)
7. ✅ `pengunduran_diri.export` - Download file (lampiran, SK, SKPB)

---

## Detail Permission per Endpoint

### 1. VIEW Permission (`pengunduran_diri.view`)

**Total**: 15 endpoints menggunakan permission ini

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri` | GET | List semua Pengunduran Diri |
| 2 | `/api/pengundurandiri/detail` | GET | Get detail lengkap |
| 3 | `/api/pengundurandiri/notif/{id}` | GET | Get notifikasi |
| 4 | `/api/pengundurandiri/riwayat` | GET | Get riwayat |
| 5 | `/api/pengundurandiri/riwayat-excel` | GET | Get riwayat Excel |
| 6 | `/api/pengundurandiri/check-report/{pdiId}` | GET | Check report |
| 7 | `/api/pengundurandiri/mahasiswa` | GET | Get mahasiswa list |
| 8 | `/api/pengundurandiri/mahasiswa/by-konsentrasi` | GET | Get mahasiswa by konsentrasi |
| 9 | `/api/pengundurandiri/prodi` | GET | Get prodi by user |
| 10 | `/api/pengundurandiri/prodi/list` | GET | Get all prodi |
| 11 | `/api/pengundurandiri/mahasiswa/{mhsId}/prodi` | GET | Get prodi mahasiswa |
| 12 | `/api/pengundurandiri/mahasiswa/{mhsId}/angkatan` | GET | Get angkatan mahasiswa |
| 13 | `/api/pengundurandiri/mahasiswa/{mhsId}/bebas-tanggungan` | GET | Cek bebas tanggungan |
| 14 | `/api/pengundurandiri/mahasiswa/{mhsId}/profil` | GET | Get profil mahasiswa |
| 15 | `/api/pengundurandiri/template-sk` | GET | Download template SK |
| 16 | `/api/pengundurandiri/template-sk/list` | GET | List template SK |
| 17 | `/api/pengundurandiri/debug-approve/{id}` | GET | Debug approve (development) |

**Kegunaan**: User dengan permission ini bisa **melihat semua data** Pengunduran Diri, termasuk list, detail, riwayat, dan data master.

---

### 2. CREATE Permission (`pengunduran_diri.create`)

**Total**: 3 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri/create` | POST | Create draft Pengunduran Diri |
| 2 | `/api/pengundurandiri/create-by-prodi` | POST | Create by Prodi (old method) |
| 3 | `/api/pengundurandiri/create-by-prodi/draft` | POST | Create draft by Prodi (STEP 1) |

**Kegunaan**: User dengan permission ini bisa **membuat pengajuan Pengunduran Diri baru**.

---

### 3. EDIT Permission (`pengunduran_diri.edit`)

**Total**: 3 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri/submit/{draftId}` | PUT | Submit draft (STEP 2) |
| 2 | `/api/pengundurandiri/create-by-prodi/submit/{draftId}` | PUT | Submit draft by Prodi (STEP 2) |
| 3 | `/api/pengundurandiri/{id}` | PUT | Update data Pengunduran Diri |

**Kegunaan**: User dengan permission ini bisa **mengedit/update data** Pengunduran Diri yang sudah ada.

---

### 4. DELETE Permission (`pengunduran_diri.delete`)

**Total**: 1 endpoint

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri/delete` | DELETE | Soft delete Pengunduran Diri |

**Kegunaan**: User dengan permission ini bisa **menghapus data** Pengunduran Diri (soft delete).

---

### 5. APPROVE_REJECT Permission (`pengunduran_diri.approve_reject`)

**Total**: 3 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri/approve` | PUT | Approve Pengunduran Diri |
| 2 | `/api/pengundurandiri/reject` | PUT | Reject Pengunduran Diri |
| 3 | `/api/pengundurandiri/debug-test-approve` | POST | Debug test approve (development) |

**Kegunaan**: User dengan permission ini bisa **approve atau reject** pengajuan Pengunduran Diri (biasanya Prodi, Wadir 1).

---

### 6. IMPORT Permission (`pengunduran_diri.import`)

**Total**: 3 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri/upload` | POST | Upload file lampiran |
| 2 | `/api/pengundurandiri/sk/{id}` | PUT | Upload SK path (old method) |
| 3 | `/api/pengundurandiri/upload-sk-file` | POST | Upload file SK & SKPB |

**Kegunaan**: User dengan permission ini bisa **upload file** (lampiran pengajuan, SK, SKPB).

---

### 7. EXPORT Permission (`pengunduran_diri.export`)

**Total**: 6 endpoints

| No | Endpoint | Method | Kegunaan |
|----|----------|--------|----------|
| 1 | `/api/pengundurandiri/file/{filename}` | GET | Download file lampiran |
| 2 | `/api/pengundurandiri/download-sk-file/{pdiId}` | GET | Download SK file by ID |
| 3 | `/api/pengundurandiri/download-sk/{pdiId}` | GET | Get SK info/path |
| 4 | `/api/pengundurandiri/download-sk-file?id={id}` | GET | Download SK file by query |
| 5 | `/api/pengundurandiri/sk/{filename}` | GET | Download SK by filename |
| 6 | `/api/pengundurandiri/skpb/{filename}` | GET | Download SKPB by filename |
| 7 | `/api/pengundurandiri/download-all-sk` | GET | Download SK + SKPB (ZIP) |

**Kegunaan**: User dengan permission ini bisa **download file** (lampiran, SK, SKPB).

---

## Rekomendasi Role Permission

### Admin
```
✅ pengunduran_diri.view
✅ pengunduran_diri.create
✅ pengunduran_diri.edit
✅ pengunduran_diri.delete
✅ pengunduran_diri.approve_reject
✅ pengunduran_diri.import
✅ pengunduran_diri.export
```
**Total**: 7 permissions (ALL)

### Wadir 1
```
✅ pengunduran_diri.view
✅ pengunduran_diri.edit
✅ pengunduran_diri.approve_reject
✅ pengunduran_diri.import
✅ pengunduran_diri.export
```
**Total**: 5 permissions

### Prodi / Sekprodi
```
✅ pengunduran_diri.view
✅ pengunduran_diri.create
✅ pengunduran_diri.approve_reject
✅ pengunduran_diri.import
✅ pengunduran_diri.export
```
**Total**: 5 permissions

### Staff Akademik
```
✅ pengunduran_diri.view
✅ pengunduran_diri.create
✅ pengunduran_diri.edit
✅ pengunduran_diri.import
✅ pengunduran_diri.export
```
**Total**: 5 permissions

### Mahasiswa
```
✅ pengunduran_diri.view (own data only)
✅ pengunduran_diri.create (own data only)
✅ pengunduran_diri.export (own data only)
```
**Total**: 3 permissions (view, create, download own data only)

---

## SQL untuk Setup Permission

### 1. Cek Permission yang Ada
```sql
SELECT * FROM sso_mspermission 
WHERE per_name LIKE 'pengunduran_diri.%'
ORDER BY per_name
```

### 2. Insert Permission (jika belum ada)
```sql
-- View
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_VIEW', 'pengunduran_diri.view', 'View Pengunduran Diri', 'Aktif')

-- Create
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_CREATE', 'pengunduran_diri.create', 'Create Pengunduran Diri', 'Aktif')

-- Edit
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_EDIT', 'pengunduran_diri.edit', 'Edit Pengunduran Diri', 'Aktif')

-- Delete
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_DELETE', 'pengunduran_diri.delete', 'Delete Pengunduran Diri', 'Aktif')

-- Approve/Reject
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_APPROVE', 'pengunduran_diri.approve_reject', 'Approve/Reject Pengunduran Diri', 'Aktif')

-- Import
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_IMPORT', 'pengunduran_diri.import', 'Upload File Pengunduran Diri', 'Aktif')

-- Export
INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
VALUES ('PER_PD_EXPORT', 'pengunduran_diri.export', 'Download File Pengunduran Diri', 'Aktif')
```

### 3. Assign Permission ke Role
```sql
-- Contoh: Assign semua permission ke Admin
INSERT INTO sso_msrolepermission (rol_id, per_id)
SELECT 'ROL_ADMIN', per_id 
FROM sso_mspermission 
WHERE per_name LIKE 'pengunduran_diri.%'

-- Contoh: Assign permission ke Prodi
INSERT INTO sso_msrolepermission (rol_id, per_id)
SELECT 'ROL_PRODI', per_id 
FROM sso_mspermission 
WHERE per_name IN (
    'pengunduran_diri.view',
    'pengunduran_diri.create',
    'pengunduran_diri.approve_reject',
    'pengunduran_diri.import',
    'pengunduran_diri.export'
)
```

### 4. Cek User Permission
```sql
-- Cek user yang punya permission Pengunduran Diri
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
WHERE p.per_name LIKE 'pengunduran_diri.%'
ORDER BY u.usr_username, p.per_name
```

---

## Perbandingan dengan Drop Out

| Aspek | Drop Out | Pengunduran Diri |
|-------|----------|------------------|
| Total Permission | 7 | 7 |
| Total Endpoints | 35 | 31 |
| VIEW endpoints | 21 | 17 |
| CREATE endpoints | 1 | 3 |
| EDIT endpoints | 2 | 3 |
| DELETE endpoints | 1 | 1 |
| APPROVE_REJECT endpoints | 2 | 3 |
| IMPORT endpoints | 2 | 3 |
| EXPORT endpoints | 6 | 7 |

**Perbedaan Utama**:
- Pengunduran Diri punya lebih banyak endpoint CREATE (3 vs 1) karena ada flow by Prodi
- Pengunduran Diri punya lebih banyak endpoint EXPORT (7 vs 6) karena ada download lampiran

---

## Summary

**Total Permission**: 7
- `pengunduran_diri.view` - 17 endpoints
- `pengunduran_diri.create` - 3 endpoints
- `pengunduran_diri.edit` - 3 endpoints
- `pengunduran_diri.delete` - 1 endpoint
- `pengunduran_diri.approve_reject` - 3 endpoints
- `pengunduran_diri.import` - 3 endpoints
- `pengunduran_diri.export` - 7 endpoints

**Total Endpoints**: 37 endpoints (termasuk debug endpoints)

**Permission Paling Banyak Digunakan**: `pengunduran_diri.view` (17 endpoints)

---

**Created**: January 28, 2026
**Status**: ✅ COMPLETE & CORRECTED
