# Permission Mapping - Drop Out & Pengunduran Diri

## Permission Standard di Database

Berdasarkan tabel `sso_mspermission`, permission standar yang ada:

| Permission Suffix | Kegunaan |
|------------------|----------|
| `VIEW` | Melihat/view data |
| `CREATE` | Membuat/tambah data baru |
| `EDIT` | Mengedit/mengubah data |
| `DELETE` | Menghapus data |
| `APPROVE_REJECT` | Approve dan Reject data |
| `EXPORT` | Download/export data (Excel, PDF, file SK, dll) |
| `IMPORT` | Upload/import data (file SK, Excel, dll) |
| `PRINT` | Print report |

**Penting**: 
- `IMPORT` = Upload file (termasuk upload SK)
- `EXPORT` = Download file (termasuk download SK)

---

## Permission Mapping Drop Out

### Format: `drop_out.{action}`

| Endpoint | Method | Permission | Kegunaan |
|----------|--------|-----------|----------|
| `/api/dropout` | GET | `drop_out.view` | List Drop Out |
| `/api/dropout/{id}` | GET | `drop_out.view` | Detail Drop Out |
| `/api/dropout/detail` | GET | `drop_out.view` | Detail lengkap |
| `/api/dropout/riwayat` | GET | `drop_out.view` | Riwayat |
| `/api/dropout/pending` | GET | `drop_out.view` | Pending list |
| `/api/dropout/prodi` | GET | `drop_out.view` | List prodi |
| `/api/dropout/mahasiswa` | GET | `drop_out.view` | List mahasiswa |
| `/api/dropout/template-sk` | GET | `drop_out.view` | View template |
| `/api/dropout/create-pengajuan` | POST | `drop_out.create` | Create pengajuan |
| `/api/dropout/{id}` | PUT | `drop_out.edit` | Update data |
| `/api/dropout/draft/{id}/generate-id` | PUT | `drop_out.edit` | Generate ID |
| `/api/dropout/wadir/approve` | PUT | `drop_out.approve_reject` | Approve by Wadir |
| `/api/dropout/wadir/reject` | PUT | `drop_out.approve_reject` | Reject by Wadir |
| `/api/dropout/{id}` | DELETE | `drop_out.delete` | Delete data |
| **`/api/dropout/upload-sk-file`** | **POST** | **`drop_out.import`** | **Upload SK & SKPB** |
| `/api/dropout/upload-sk` | PUT | `drop_out.import` | Upload SK path |
| **`/api/dropout/download-sk-file/{id}`** | **GET** | **`drop_out.export`** | **Download SK file** |
| `/api/dropout/sk/{filename}` | GET | `drop_out.export` | Download SK by filename |
| `/api/dropout/skpb/{filename}` | GET | `drop_out.export` | Download SKPB by filename |
| `/api/dropout/download-all-sk` | GET | `drop_out.export` | Download SK + SKPB (ZIP) |
| `/api/dropout/riwayat/excel` | GET | `drop_out.export` | Export riwayat Excel |

---

## Permission Mapping Pengunduran Diri

### Format: `pengunduran_diri.{action}`

| Endpoint | Method | Permission | Kegunaan |
|----------|--------|-----------|----------|
| `/api/pengundurandiri` | GET | `pengunduran_diri.view` | List Pengunduran Diri |
| `/api/pengundurandiri/detail` | GET | `pengunduran_diri.view` | Detail |
| `/api/pengundurandiri/riwayat` | GET | `pengunduran_diri.view` | Riwayat |
| `/api/pengundurandiri/prodi` | GET | `pengunduran_diri.view` | List prodi |
| `/api/pengundurandiri/mahasiswa` | GET | `pengunduran_diri.view` | List mahasiswa |
| `/api/pengundurandiri/template-sk` | GET | `pengunduran_diri.view` | View template |
| `/api/pengundurandiri/create` | POST | `pengunduran_diri.create` | Create draft |
| `/api/pengundurandiri/submit/{id}` | PUT | `pengunduran_diri.create` | Submit draft |
| `/api/pengundurandiri/{id}` | PUT | `pengunduran_diri.edit` | Update data |
| `/api/pengundurandiri/approve` | PUT | `pengunduran_diri.approve_reject` | Approve |
| `/api/pengundurandiri/reject` | PUT | `pengunduran_diri.approve_reject` | Reject |
| `/api/pengundurandiri/delete` | DELETE | `pengunduran_diri.delete` | Soft delete |
| **`/api/pengundurandiri/upload-sk-file`** | **POST** | **`pengunduran_diri.import`** | **Upload SK & SKPB** |
| **`/api/pengundurandiri/download-sk-file/{id}`** | **GET** | **`pengunduran_diri.export`** | **Download SK file** |
| `/api/pengundurandiri/sk/{filename}` | GET | `pengunduran_diri.export` | Download SK by filename |
| `/api/pengundurandiri/skpb/{filename}` | GET | `pengunduran_diri.export` | Download SKPB by filename |
| `/api/pengundurandiri/download-all-sk` | GET | `pengunduran_diri.export` | Download SK + SKPB (ZIP) |
| `/api/pengundurandiri/riwayat-excel` | GET | `pengunduran_diri.export` | Export riwayat Excel |

---

## Penjelasan Permission

### IMPORT vs EXPORT

**IMPORT** = Upload/Import data ke sistem:
- Upload file SK
- Upload file SKPB
- Import data dari Excel
- Upload dokumen pendukung

**EXPORT** = Download/Export data dari sistem:
- Download file SK
- Download file SKPB
- Export data ke Excel
- Download report PDF

### EDIT vs IMPORT

**EDIT** = Mengubah data yang sudah ada:
- Update informasi mahasiswa
- Update status
- Update keterangan

**IMPORT** = Menambahkan file/data baru:
- Upload file SK (file baru)
- Upload file SKPB (file baru)
- Import data Excel

---

## SQL Check Permission

### Cek Permission Drop Out
```sql
-- Cek semua permission Drop Out
SELECT * FROM sso_mspermission 
WHERE per_name LIKE 'drop_out.%'
ORDER BY per_name

-- Expected results:
-- drop_out.view
-- drop_out.create
-- drop_out.edit
-- drop_out.delete
-- drop_out.approve_reject
-- drop_out.import
-- drop_out.export
```

### Cek Permission Pengunduran Diri
```sql
-- Cek semua permission Pengunduran Diri
SELECT * FROM sso_mspermission 
WHERE per_name LIKE 'pengunduran_diri.%'
ORDER BY per_name

-- Expected results:
-- pengunduran_diri.view
-- pengunduran_diri.create
-- pengunduran_diri.edit
-- pengunduran_diri.delete
-- pengunduran_diri.approve_reject
-- pengunduran_diri.import
-- pengunduran_diri.export
```

### Cek User Permission
```sql
-- Cek user yang bisa upload SK Drop Out (punya permission import)
SELECT u.usr_username, r.rol_name, p.per_name
FROM sso_msuserrole ur
INNER JOIN sso_msuser u ON ur.usr_id = u.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
INNER JOIN sso_msrolepermission rp ON r.rol_id = rp.rol_id
INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
WHERE p.per_name = 'drop_out.import'

-- Cek user yang bisa download SK Drop Out (punya permission export)
SELECT u.usr_username, r.rol_name, p.per_name
FROM sso_msuserrole ur
INNER JOIN sso_msuser u ON ur.usr_id = u.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
INNER JOIN sso_msrolepermission rp ON r.rol_id = rp.rol_id
INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
WHERE p.per_name = 'drop_out.export'
```

---

## Role yang Biasanya Punya Permission

| Role | Permissions | Upload SK? | Download SK? |
|------|------------|-----------|-------------|
| Admin | All | ✅ Yes (import) | ✅ Yes (export) |
| Wadir 1 | view, edit, approve_reject, import, export | ✅ Yes | ✅ Yes |
| Staff Akademik | view, create, edit, import, export | ✅ Yes | ✅ Yes |
| Sekprodi | view, create, export | ❌ No | ✅ Yes (download only) |
| Mahasiswa | view (own data), export (own data) | ❌ No | ✅ Yes (own data) |

---

## Summary

✅ **Upload SK** = Permission `IMPORT`
- `drop_out.import` untuk upload SK Drop Out
- `pengunduran_diri.import` untuk upload SK Pengunduran Diri

✅ **Download SK** = Permission `EXPORT`
- `drop_out.export` untuk download SK Drop Out
- `pengunduran_diri.export` untuk download SK Pengunduran Diri

✅ **View Data** = Permission `VIEW`
✅ **Create Data** = Permission `CREATE`
✅ **Edit Data** = Permission `EDIT`
✅ **Delete Data** = Permission `DELETE`
✅ **Approve/Reject** = Permission `APPROVE_REJECT`

---

**Updated**: January 28, 2026
**Status**: ✅ CORRECTED (IMPORT for upload, EXPORT for download)

