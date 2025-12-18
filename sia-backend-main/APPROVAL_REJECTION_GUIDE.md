# Panduan Approval & Rejection Cuti Akademik

## 🚀 Endpoints yang Tersedia

### 1. Approve Cuti (General)
**Endpoint:** `PUT /api/CutiAkademik/approve`

**Untuk:** Prodi, Wadir 1, Finance

**Request Body:**
```json
{
  "id": "012/PMA/CA/IX/2025",
  "role": "prodi",
  "approvedBy": "andreas_e"
}
```

**Role yang valid:**
- `"prodi"` → Status: "Belum Disetujui Wadir 1"
- `"wadir1"` → Status: "Belum Disetujui Finance"  
- `"finance"` → Status: "Menunggu Upload SK" + generate nomor surat

### 2. Approve Prodi (Khusus)
**Endpoint:** `PUT /api/CutiAkademik/approve/prodi`

**Untuk:** Prodi (tanpa menimbang)

**Request Body:**
```json
{
  "id": "012/PMA/CA/IX/2025",
  "approvedBy": "andreas_e"
}
```

### 3. Reject Cuti
**Endpoint:** `PUT /api/CutiAkademik/reject`

**Untuk:** Prodi, Wadir 1, Finance (tanpa keterangan)

**Request Body:**
```json
{
  "id": "012/PMA/CA/IX/2025",
  "role": "prodi"
}
```

## 📋 Cara Testing di Swagger

### 1. Approve oleh Prodi
```
PUT /api/CutiAkademik/approve

Body:
{
  "id": "012/PMA/CA/IX/2025",
  "role": "prodi",
  "approvedBy": "andreas_e"
}

Expected Response:
{
  "message": "Cuti akademik berhasil disetujui."
}
```

### 2. Approve oleh Wadir 1
```
PUT /api/CutiAkademik/approve

Body:
{
  "id": "012/PMA/CA/IX/2025",
  "role": "wadir1",
  "approvedBy": "tonny.pongoh"
}
```

### 3. Approve oleh Finance
```
PUT /api/CutiAkademik/approve

Body:
{
  "id": "012/PMA/CA/IX/2025",
  "role": "finance",
  "approvedBy": "finance_user"
}
```

### 4. Approve Prodi (Sederhana)
```
PUT /api/CutiAkademik/approve/prodi

Body:
{
  "id": "012/PMA/CA/IX/2025",
  "approvedBy": "andreas_e"
}
```

### 5. Reject oleh Prodi (Sederhana)
```
PUT /api/CutiAkademik/reject

Body:
{
  "id": "012/PMA/CA/IX/2025",
  "role": "prodi"
}
```

## 🔄 Flow Approval

### Normal Flow
```
Draft → Generate Final ID → Belum Disetujui Prodi → Belum Disetujui Wadir 1 → Belum Disetujui Finance → Menunggu Upload SK → Disetujui
```

### Detailed Steps
1. **Mahasiswa/Prodi** create draft
2. **Generate Final ID** → Status: "Belum Disetujui Prodi" (jika dari mahasiswa) atau "Belum Disetujui Wadir 1" (jika dari prodi)
3. **Prodi Approve** → Status: "Belum Disetujui Wadir 1"
4. **Wadir 1 Approve** → Status: "Belum Disetujui Finance"
5. **Finance Approve** → Status: "Menunggu Upload SK" + generate nomor surat
6. **Upload SK** → Status: "Disetujui"

### Rejection Flow
- **Ditolak Prodi** → Status: "Ditolak prodi"
- **Ditolak Wadir 1** → Status: "Ditolak wadir1"  
- **Ditolak Finance** → Status: "Ditolak finance"

## 🎯 Stored Procedures yang Digunakan

### 1. sia_setujuiCutiAkademik
**Parameters:**
- `@p1` = ID cuti akademik
- `@p2` = Role ("prodi", "wadir1", "finance")
- `@p3` = Username yang menyetujui

**Logic:**
- **prodi**: Set `cak_approval_prodi`, `cak_status = 'Belum Disetujui Wadir 1'`, `cak_app_prodi_date = GETDATE()`
- **wadir1**: Set `cak_approval_dir1`, `cak_status = 'Belum Disetujui Finance'`, `cak_app_dir1_date = GETDATE()`
- **finance**: Set `srt_no`, `cak_approval_dakap`, `cak_status = 'Menunggu Upload SK'`, `cak_app_dakap_date = GETDATE()`, generate nomor surat

### 2. sia_setujuiCutiAkademikProdi
**Parameters:**
- `@p1` = ID cuti akademik
- `@p2` = Menimbang/pertimbangan
- `@p3` = Username prodi

**Logic:**
- Update `cak_menimbang = @p2`, `cak_approval_prodi = @p3`, `cak_status = 'Belum Disetujui Wadir 1'`, `cak_app_prodi_date = GETDATE()`

### 3. sia_tolakCutiAkademik
**Parameters:**
- `@p1` = ID cuti akademik
- `@p2` = Role yang menolak
- `@p3` = Keterangan penolakan

**Logic:**
- Update `cak_keterangan = @p3`, `cak_status = 'Ditolak ' + @p2`

## 🧪 Testing Scenarios

### Scenario 1: Complete Approval Flow
1. Create draft → Generate final ID
2. Prodi approve → Status: "Belum Disetujui Wadir 1"
3. Wadir 1 approve → Status: "Belum Disetujui Finance"
4. Finance approve → Status: "Menunggu Upload SK"

### Scenario 2: Prodi Rejection
1. Create draft → Generate final ID
2. Prodi reject → Status: "Ditolak prodi"

### Scenario 3: Prodi Approve with Menimbang
1. Create draft → Generate final ID
2. Use `/approve/prodi` endpoint with menimbang
3. Status: "Belum Disetujui Wadir 1"

## 📝 Console Logs

**Successful Approval:**
```
Approve request - ID: 012/PMA/CA/IX/2025, Role: prodi, ApprovedBy: andreas_e
Approval successful
```

**Successful Rejection:**
```
Reject request - ID: 012/PMA/CA/IX/2025, Role: prodi
Rejection successful
```

## 🔧 Troubleshooting

### Error 400 Bad Request
1. **Check ID**: Pastikan ID cuti akademik valid
2. **Check Role**: Pastikan role valid ("prodi", "wadir1", "finance")
3. **Check Status**: Pastikan status cuti sesuai untuk approval/rejection

### Error 500 Internal Server Error
1. **Check SP**: Pastikan stored procedure ada di database
2. **Check Database**: Pastikan koneksi database OK
3. **Check Console**: Lihat error message di console

### Approval Gagal
1. **Check Current Status**: Pastikan status cuti sesuai untuk di-approve
2. **Check User Permission**: Pastikan user punya hak approve
3. **Check ID Format**: Pastikan menggunakan final ID, bukan draft ID

Sekarang sistem approval dan rejection cuti akademik sudah lengkap dan siap digunakan!