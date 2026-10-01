# Frontend Guide - Drop Out Menimbang & Mengingat

## ✅ Status Frontend: SUDAH BENAR

Berdasarkan testing yang sudah dilakukan, **Frontend sudah mengirim data dengan benar**. Tidak ada perubahan yang diperlukan di sisi Frontend.

## 📋 Checklist Verifikasi Frontend

### 1. Request Payload (POST Create Pengajuan)

**Endpoint**: `POST /api/DropOut/create-pengajuan`

**Payload yang SUDAH BENAR**:
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

### 2. Field Names (Case Sensitive)

Pastikan field names menggunakan **camelCase** yang benar:

| ✅ BENAR | ❌ SALAH |
|----------|----------|
| `mhsId` | `MhsId` atau `mhs_id` |
| `menimbang` | `Menimbang` atau `dro_menimbang` |
| `mengingat` | `Mengingat` atau `dro_mengingat` |
| `lampiran` | `Lampiran` atau `dro_lampiran` |
| `lampiranSuratPengajuan` | `LampiranSuratPengajuan` atau `lampiran_surat_pengajuan` |
| `createdBy` | `CreatedBy` atau `created_by` |

### 3. Content Type

Pastikan request menggunakan:
```
Content-Type: application/json
```

### 4. HTML Content

Frontend **BOLEH** mengirim HTML content dengan tags seperti:
- `<p>`, `<br>`, `<div>`, `<span>`
- `<strong>`, `<em>`, `<u>`
- `<ul>`, `<ol>`, `<li>`

Backend **TIDAK** melakukan HTML sanitization yang menghapus content.

---

## 🧪 Testing Guide untuk Frontend

### Test Case 1: Create Draft dengan Menimbang & Mengingat

**Request**:
```javascript
// JavaScript/TypeScript Example
const payload = {
  mhsId: "0720250061",
  menimbang: "<p>Menimbang bahwa mahasiswa telah mengajukan permohonan...</p>",
  mengingat: "<p>Mengingat peraturan akademik yang berlaku...</p>",
  lampiran: "",
  lampiranSuratPengajuan: "",
  createdBy: "nda_prodi"
};

const response = await fetch('/api/DropOut/create-pengajuan', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify(payload)
});

const result = await response.json();
console.log(result);
```

**Expected Response**:
```json
{
  "message": "Pengajuan DO Draft berhasil dibuat.",
  "id": "5",
  "createdBy": "nda_prodi"
}
```

### Test Case 2: Verify Data Tersimpan

Setelah create, ambil detail untuk memverifikasi data tersimpan:

**Request**:
```javascript
const droId = "5"; // ID dari response create
const response = await fetch(`/api/DropOut/detail?id=${droId}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const detail = await response.json();
console.log('Menimbang:', detail.menimbang);
console.log('Mengingat:', detail.mengingat);
```

**Expected Response**:
```json
{
  "id": "5",
  "mhsId": "0720250061",
  "mhsText": "0720250061 - MUHAMAD RIDWAN FATUR RIZKI",
  "prodi": "Teknologi Rekayasa Pemeliharaan Alat Berat",
  "angkatan": "2025",
  "status": "Draft",
  "menimbang": "<p>Menimbang bahwa mahasiswa telah mengajukan permohonan...</p>",
  "mengingat": "<p>Mengingat peraturan akademik yang berlaku...</p>",
  "createdBy": "nda_prodi"
}
```

---

## 🔍 Troubleshooting

### Issue 1: Field menimbang/mengingat masih null/empty

**Kemungkinan Penyebab**:
1. ❌ Field name salah (case sensitive)
2. ❌ Content-Type bukan `application/json`
3. ❌ Backend belum di-restart setelah update

**Solusi**:
1. ✅ Pastikan field name menggunakan camelCase: `menimbang`, `mengingat`
2. ✅ Pastikan Content-Type: `application/json`
3. ✅ Restart backend API

### Issue 2: HTML tags hilang

**Kemungkinan Penyebab**:
1. ❌ Frontend melakukan HTML encoding sebelum kirim
2. ❌ Database column terlalu kecil

**Solusi**:
1. ✅ Kirim HTML as-is, jangan di-encode
2. ✅ Pastikan database column type: `TEXT` atau `VARCHAR(MAX)`

### Issue 3: Response 400 Bad Request

**Kemungkinan Penyebab**:
1. ❌ JSON format salah
2. ❌ Required field kosong (mhsId)
3. ❌ Authorization token tidak valid

**Solusi**:
1. ✅ Validate JSON format
2. ✅ Pastikan mhsId tidak kosong
3. ✅ Pastikan token valid dan tidak expired

---

## 📝 Code Examples

### React/TypeScript Example

```typescript
interface CreateDropOutRequest {
  mhsId: string;
  menimbang: string;
  mengingat: string;
  lampiran: string;
  lampiranSuratPengajuan: string;
  createdBy?: string;
}

const createDropOutDraft = async (data: CreateDropOutRequest) => {
  try {
    const response = await fetch('/api/DropOut/create-pengajuan', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();
    console.log('Draft created:', result);
    return result;
  } catch (error) {
    console.error('Error creating draft:', error);
    throw error;
  }
};

// Usage
const formData = {
  mhsId: "0720250061",
  menimbang: editorMenimbang.getHTML(), // dari rich text editor
  mengingat: editorMengingat.getHTML(), // dari rich text editor
  lampiran: "",
  lampiranSuratPengajuan: ""
};

await createDropOutDraft(formData);
```

### Vue.js Example

```javascript
// Vue 3 Composition API
import { ref } from 'vue';

const createDropOut = async () => {
  const payload = {
    mhsId: mhsId.value,
    menimbang: menimbangContent.value,
    mengingat: mengingatContent.value,
    lampiran: lampiran.value || "",
    lampiranSuratPengajuan: lampiranSuratPengajuan.value || ""
  };

  try {
    const response = await fetch('/api/DropOut/create-pengajuan', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token.value}`
      },
      body: JSON.stringify(payload)
    });

    const result = await response.json();
    
    if (response.ok) {
      console.log('Success:', result);
      // Redirect atau show success message
    } else {
      console.error('Error:', result);
      // Show error message
    }
  } catch (error) {
    console.error('Network error:', error);
  }
};
```

### Angular Example

```typescript
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DropOutService {
  private apiUrl = '/api/DropOut';

  constructor(private http: HttpClient) {}

  createPengajuan(data: CreateDropOutRequest): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.getToken()}`
    });

    return this.http.post(
      `${this.apiUrl}/create-pengajuan`,
      data,
      { headers }
    );
  }

  private getToken(): string {
    // Get token from storage
    return localStorage.getItem('token') || '';
  }
}

// Usage in component
this.dropOutService.createPengajuan({
  mhsId: this.form.value.mhsId,
  menimbang: this.form.value.menimbang,
  mengingat: this.form.value.mengingat,
  lampiran: this.form.value.lampiran || "",
  lampiranSuratPengajuan: this.form.value.lampiranSuratPengajuan || ""
}).subscribe({
  next: (result) => {
    console.log('Success:', result);
  },
  error: (error) => {
    console.error('Error:', error);
  }
});
```

---

## ✅ Kesimpulan

**Frontend TIDAK PERLU DIUBAH** karena sudah mengirim data dengan format yang benar.

Yang sudah diperbaiki di **Backend**:
1. ✅ DTO sudah ditambahkan field `Menimbang` dan `Mengingat`
2. ✅ Repository sudah menyimpan field tersebut ke database
3. ✅ Backend siap menerima dan menyimpan data dari Frontend

**Next Steps**:
1. Deploy backend yang sudah diupdate
2. Test dari Frontend untuk memverifikasi data tersimpan
3. Jika masih ada issue, cek troubleshooting guide di atas

---

## 📞 Support

Jika masih ada masalah setelah backend di-deploy, silakan hubungi backend team dengan informasi:
- Request payload yang dikirim
- Response yang diterima
- Error message (jika ada)
- Browser console logs
