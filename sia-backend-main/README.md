# 🚀 SIA Backend Repository

Repository ini digunakan sebagai **pengembangan lanjutan (development branch)** dari proyek **Astratech Apps Backend**.  
Semua perubahan dan eksperimen dilakukan di repo ini dan di branch **dev**

---

## 📦 Tujuan Repository
Repo **SIA Backend** berfungsi untuk:
- Menyediakan pipeline CI/CD dan konfigurasi Docker terbaru.
- Menggantikan file `.gitlab-ci.yml` dan `Dockerfile` lama yang ada di repo **Astratech Apps Backend**.
- Menjadi tempat kolaborasi bagi developer untuk melakukan pengembangan di branch `dev`.

---

## ⚙️ Langkah Integrasi
1. **Clone repo SIA Backend**
   ```bash
   git clone <URL_SIA_Backend_REPO>

## 👩‍💻 Aturan Penggunaan Repo SIA Backend
1. **Branch Rules**

    Gunakan branch dev untuk semua aktivitas development.
    **Tidak diperbolehkan melakukan push** ke branch:

        staging
        main

    **Merge ke staging atau main hanya boleh dilakukan oleh maintainer atau admin setelah review.**

2. **Commit Guidelines**

    Gunakan format commit message yang jelas dan konsisten.

        feat: menambahkan fitur login baru

        fix: memperbaiki bug pada endpoint /auth
    
        chore: memperbarui dependensi {apa}


    Pastikan setelah melakukan push, untuk mengecek hasil dari sonarqube.
    Jika ada issue segera perbaiki karena nanti akan mempengaruhi untuk kelompok lain.

## 🧭 Catatan Tambahan

Pastikan .gitlab-ci.yml dan Dockerfile di repo Astratech Apps Backend diganti dengan yang ada di repo SIA Backend ini.