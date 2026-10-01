# Frontend Guide - Drop Out Menimbang & Mengingat

## ✅ GOOD NEWS: Frontend TIDAK PERLU DIUBAH!

Berdasarkan evidence yang Anda berikan, **Frontend sudah benar** mengirim data `menimbang` dan `mengingat`:

```json
{
  "mhsId": "0720250061",
  "menimbang": "<p>dinf</p>",
  "mengingat": "<p>jhuygftgv hjbgytfr fvbhbjgytfdncf</p>",
  "lampiran": "",
  "lampiranSuratPengajuan": "",
  "createdBy": "nda_prodi"
}
```

Backend sekarang sudah diperbaiki untuk menerima dan menyimpan field tersebut.

---

## 🧪 Testing dari Frontend

### Test 1: Create Draft DO dengan Menimbang & Mengingat

**Endpoint**: `POST /api/DropOut/create-pengajuan`

**Request Body** (yang sudah benar dari FE):
```json
{
  "mhsId": "0720250061",
  "menimbang": "<p>Bahwa mahasiswa tersebut mengajukan permohonan drop out</p>",
  "mengingat": "<p>Peraturan akademik tentang drop out mahasiswa</p>",
  "lampiran": "",
  "lampiranSuratPengajuan": "",
  "createdBy": "nda_prodi"
}
```

**Expected Response**:
```json
{
  "message": "Pengajuan DO Draft berhasil dibuat.",
  "id": "5",
  "createdBy": "nda_prodi"
}
```

### Test 2: Verify Data Tersimpan

**Endpoint**: `GET /api/DropOut/detail?id=5`

**Expected Response** (sekarang harus ada menimbang & mengingat):
```json
{
  "id": "5",
  "mhsId": "0720250061",
  "mhsText": "0720250061 - MUHAMAD RIDWAN FATUR RIZKI",
  "prodi": "Teknologi Rekayasa Pemeliharaan Alat Berat",
  "angkatan": "2025",
  "status": "Draft",
  "menimbang": "<p>Bahwa mahasiswa tersebut mengajukan permohonan drop out</p>",
  "mengingat": "<p>Peraturan akademik tentang drop out mahasiswa</p>",
  "createdBy": "nda_prodi"
}
```

---

## 📝 Jika Frontend Menggunakan Form/State Management

### React/Vue/Angular Example

```javascript
// State untuk form
const [formData, setFormData] = useState({
  mhsId: '',
  menimbang: '',      // ✅ Sudah ada
  mengingat: '',      // ✅ Sudah ada
  lampiran: '',
  lampiranSuratPengajuan: '',
  createdBy: ''
});

// Submit handler
const handleSubmit = async () => {
  try {
    const response = await axios.post('/api/DropOut/create-pengajuan', {
      mhsId: formData.mhsId,
      menimbang: formData.menimbang,      // ✅ Kirim ke backend
      mengingat: formData.mengingat,      // ✅ Kirim ke backend
      lampiran: formData.lampiran,
      lampiranSuratPengajuan: formData.lampiranSuratPengajuan,
      createdBy: formData.createdBy
    });
    
    console.log('Success:', response.data);
    // Response: { message: "...", id: "5", createdBy: "..." }
  } catch (error) {
    console.error('Error:', error);
  }
};
```

### HTML Form Example (jika menggunakan rich text editor)

```html
<!-- Menimbang Field -->
<div class="form-group">
  <label>Menimbang</label>
  <div id="editor-menimbang"></div>
  <!-- Rich text editor seperti Quill, TinyMCE, CKEditor -->
</div>

<!-- Mengingat Field -->
<div class="form-group">
  <label>Mengingat</label>
  <div id="editor-mengingat"></div>
  <!-- Rich text editor seperti Quill, TinyMCE, CKEditor -->
</div>
```

### Quill Editor Example

```javascript
// Initialize Quill editors
const quillMenimbang = new Quill('#editor-menimbang', {
  theme: 'snow',
  placeholder: 'Masukkan menimbang...'
});

const quillMengingat = new Quill('#editor-mengingat', {
  theme: 'snow',
  placeholder: 'Masukkan mengingat...'
});

// Get HTML content when submitting
const menimbangHTML = quillMenimbang.root.innerHTML;
const mengingatHTML = quillMengingat.root.innerHTML;

// Send to backend
const payload = {
  mhsId: selectedMahasiswa,
  menimbang: menimbangHTML,    // HTML content
  mengingat: mengingatHTML,    // HTML content
  lampiran: '',
  lampiranSuratPengajuan: '',
  createdBy: currentUser
};
```

---

## 🔍 Debugging Tips

### 1. Check Request Payload di Browser DevTools

```javascript
// Buka Network tab di Chrome DevTools
// Filter: XHR/Fetch
// Cari request ke: /api/DropOut/create-pengajuan
// Lihat Request Payload, pastikan ada:
{
  "menimbang": "<p>...</p>",
  "mengingat": "<p>...</p>"
}
```

### 2. Check Response dari Backend

```javascript
// Response harus 200 OK dengan:
{
  "message": "Pengajuan DO Draft berhasil dibuat.",
  "id": "5",
  "createdBy": "nda_prodi"
}
```

### 3. Verify Data dengan GET Detail

```javascript
// Setelah create, langsung GET detail
const response = await axios.get(`/api/DropOut/detail?id=${newId}`);
console.log('Menimbang:', response.data.menimbang);  // Harus ada isinya
console.log('Mengingat:', response.data.mengingat);  // Harus ada isinya
```

---

## ⚠️ Common Issues & Solutions

### Issue 1: Menimbang/Mengingat masih null setelah create

**Penyebab**: Backend belum di-restart setelah update code

**Solusi**:
```bash
# Restart backend API
dotnet run
# atau
dotnet watch run
```

### Issue 2: HTML tags hilang

**Penyebab**: Backend melakukan HTML sanitization

**Solusi**: Pastikan backend tidak melakukan sanitization pada field ini (sudah OK di code yang baru)

### Issue 3: Field terlalu panjang

**Penyebab**: Database column terlalu kecil

**Solusi**: Pastikan column `dro_menimbang` dan `dro_mengingat` adalah `VARCHAR(MAX)` atau `TEXT`

---

## 📊 Expected Behavior

| Action | Before Fix | After Fix |
|--------|-----------|-----------|
| POST create-pengajuan | ✅ 200 OK | ✅ 200 OK |
| Data menimbang saved | ❌ NULL | ✅ Saved |
| Data mengingat saved | ❌ NULL | ✅ Saved |
| GET detail shows data | ❌ Empty | ✅ Shows HTML |

---

## 🎯 Summary

**Frontend TIDAK PERLU DIUBAH** karena:
1. ✅ Frontend sudah mengirim `menimbang` dan `mengingat` dengan benar
2. ✅ Backend sekarang sudah menerima dan menyimpan field tersebut
3. ✅ GET detail sekarang akan mengembalikan data yang tersimpan

**Yang perlu dilakukan**:
1. Deploy/restart backend dengan code yang sudah diperbaiki
2. Test ulang dari frontend
3. Verify data tersimpan dengan GET detail

---

## 📞 Contact

Jika masih ada masalah setelah backend di-update, cek:
1. Backend logs untuk error
2. Database untuk memastikan data tersimpan
3. Network tab di browser untuk melihat request/response

**Status**: ✅ READY - Backend sudah siap menerima data dari frontend!
