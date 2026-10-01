# Frontend Quick Reference - Drop Out API

## ✅ TIDAK ADA PERUBAHAN DIPERLUKAN DI FRONTEND

Frontend sudah mengirim data dengan benar. Backend sudah diperbaiki untuk menerima dan menyimpan field `menimbang` dan `mengingat`.

---

## 📋 API Contract

### POST /api/DropOut/create-pengajuan

**Request Body**:
```json
{
  "mhsId": "string (required)",
  "menimbang": "string (HTML content)",
  "mengingat": "string (HTML content)",
  "lampiran": "string (optional)",
  "lampiranSuratPengajuan": "string (optional)",
  "createdBy": "string (optional, dari JWT)"
}
```

**Response 200 OK**:
```json
{
  "message": "Pengajuan DO Draft berhasil dibuat.",
  "id": "5",
  "createdBy": "nda_prodi"
}
```

---

## 🔑 Field Names (Case Sensitive!)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `mhsId` | string | ✅ Yes | ID Mahasiswa |
| `menimbang` | string | ❌ No | HTML content |
| `mengingat` | string | ❌ No | HTML content |
| `lampiran` | string | ❌ No | Path/URL lampiran |
| `lampiranSuratPengajuan` | string | ❌ No | Path/URL surat |
| `createdBy` | string | ❌ No | Auto dari JWT |

---

## ✅ Checklist Sebelum Deploy

- [ ] Field names menggunakan **camelCase** (bukan PascalCase atau snake_case)
- [ ] Content-Type: `application/json`
- [ ] Authorization header dengan Bearer token
- [ ] HTML content dikirim as-is (tidak di-encode)
- [ ] Backend sudah di-restart setelah update

---

## 🧪 Quick Test

```bash
# Test dengan curl
curl -X POST "https://your-api.com/api/DropOut/create-pengajuan" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "mhsId": "0720250061",
    "menimbang": "<p>Test menimbang</p>",
    "mengingat": "<p>Test mengingat</p>",
    "lampiran": "",
    "lampiranSuratPengajuan": ""
  }'
```

**Expected**: Status 200, response dengan `id` dan `message`

---

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| Field null/empty | Check field names (case sensitive) |
| 400 Bad Request | Validate JSON format & required fields |
| 401 Unauthorized | Check token validity |
| HTML tags hilang | Don't encode HTML before sending |

---

## 📞 Need Help?

Lihat dokumentasi lengkap di: `FRONTEND_GUIDE_DROPOUT.md`
