# Quick Reference - Drop Out Menimbang & Mengingat Fix

## 🎯 TL;DR (Too Long; Didn't Read)

**Frontend**: ✅ TIDAK PERLU DIUBAH - sudah benar!  
**Backend**: ✅ SUDAH DIPERBAIKI - tinggal restart  
**Database**: ⚠️ Pastikan kolom `dro_menimbang` dan `dro_mengingat` ada

---

## 📋 Checklist

### Backend Developer
- [x] Update DTO `CreatePengajuanDORequest` - tambah `Menimbang` & `Mengingat`
- [x] Update Repository `CreatePengajuanDOAsync` - tambah UPDATE setelah INSERT
- [ ] Restart backend API
- [ ] Test endpoint dengan Swagger/Postman

### Frontend Developer
- [ ] **TIDAK ADA YANG PERLU DIUBAH** - frontend sudah benar!
- [ ] Test ulang setelah backend di-restart
- [ ] Verify data tersimpan dengan GET detail

### Database Admin
- [ ] Cek kolom `dro_menimbang` dan `dro_mengingat` ada di tabel `sia_msdropout`
- [ ] Pastikan tipe data VARCHAR(MAX) atau TEXT

---

## 🧪 Quick Test

### 1. Test dari Swagger/Postman

**POST** `/api/DropOut/create-pengajuan`
```json
{
  "mhsId": "0720250061",
  "menimbang": "<p>Test menimbang</p>",
  "mengingat": "<p>Test mengingat</p>",
  "lampiran": "",
  "lampiranSuratPengajuan": "",
  "createdBy": "nda_prodi"
}
```

**Expected**: Status 200, response dengan `id` baru

### 2. Verify Data Tersimpan

**GET** `/api/DropOut/detail?id={id_dari_step_1}`

**Expected**: Response harus include:
```json
{
  "menimbang": "<p>Test menimbang</p>",
  "mengingat": "<p>Test mengingat</p>"
}
```

---

## 🔍 Cek User str_main_id

### Quick Query
```sql
-- Cek user berdasarkan username
SELECT str_main_id, str_main_namaakun, str_main_nama, str_main_email
FROM sia_msstruktur
WHERE str_main_namaakun = 'nda_prodi';
```

### Cek Permissions
```sql
-- Cek apakah user punya permission drop_out
SELECT s.str_main_namaakun, p.permission_code
FROM sia_msstruktur s
LEFT JOIN sia_msrole r ON s.str_main_role_id = r.role_id
LEFT JOIN sia_msrole_permission rp ON r.role_id = rp.role_id
LEFT JOIN sia_mspermission p ON rp.permission_id = p.permission_id
WHERE s.str_main_namaakun = 'nda_prodi'
  AND p.permission_code LIKE 'drop_out%';
```

---

## 🐛 Troubleshooting

### Problem: Menimbang/Mengingat masih NULL

**Solution 1**: Restart backend
```bash
# Stop backend (Ctrl+C)
# Start again
dotnet run
```

**Solution 2**: Cek database
```sql
-- Cek apakah kolom ada
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'sia_msdropout'
  AND COLUMN_NAME IN ('dro_menimbang', 'dro_mengingat');
```

**Solution 3**: Cek data langsung di database
```sql
-- Cek data yang baru dibuat
SELECT TOP 1 
    dro_id, 
    mhs_id, 
    dro_menimbang, 
    dro_mengingat,
    dro_created_by,
    dro_created_date
FROM sia_msdropout
ORDER BY dro_created_date DESC;
```

### Problem: Error 500 saat POST

**Check**: Backend logs untuk error message

**Common causes**:
1. Database connection issue
2. User tidak punya permission
3. Kolom database tidak ada

---

## 📁 Files Changed

1. `DTOs/DropOut/CreatePengajuanDORequest.cs` - Added `Menimbang` & `Mengingat`
2. `Repositories/Implementations/DropOutRepository.cs` - Added UPDATE after INSERT
3. `DROPOUT_MENIMBANG_MENGINGAT_FIX.md` - Documentation
4. `FRONTEND_DROPOUT_MENIMBANG_MENGINGAT.md` - Frontend guide
5. `SQL_CHECK_USER_STR_MAIN_ID.sql` - SQL queries

---

## 📞 Need Help?

1. Check `DROPOUT_MENIMBANG_MENGINGAT_FIX.md` for detailed explanation
2. Check `FRONTEND_DROPOUT_MENIMBANG_MENGINGAT.md` for frontend guide
3. Check `SQL_CHECK_USER_STR_MAIN_ID.sql` for database queries
4. Check backend logs for errors
5. Check database for data

---

## ✅ Success Criteria

- [x] POST create-pengajuan returns 200 OK
- [x] Response includes new `id`
- [x] GET detail shows `menimbang` with HTML content
- [x] GET detail shows `mengingat` with HTML content
- [x] Data persists in database

**Status**: ✅ READY FOR TESTING
