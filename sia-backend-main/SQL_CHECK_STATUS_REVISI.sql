-- =============================================
-- Query untuk Cek Status "Revisi" di Tabel sia_msdropout
-- =============================================

-- 1. CEK APAKAH ADA DATA DENGAN STATUS "REVISI"
SELECT 
    dro_id,
    mhs_id,
    dro_status,
    dro_created_by,
    dro_created_date,
    dro_modif_by,
    dro_modif_date
FROM sia_msdropout
WHERE dro_status = 'Revisi'
ORDER BY dro_created_date DESC;

-- 2. CEK SEMUA STATUS YANG ADA DI DATABASE (SUMMARY)
SELECT 
    dro_status,
    COUNT(*) as jumlah_data
FROM sia_msdropout
GROUP BY dro_status
ORDER BY jumlah_data DESC;

-- 3. CEK STATUS YANG MENGANDUNG KATA "REVISI" (CASE INSENSITIVE)
SELECT 
    dro_id,
    mhs_id,
    dro_status,
    dro_created_by,
    dro_created_date
FROM sia_msdropout
WHERE LOWER(dro_status) LIKE '%revisi%'
ORDER BY dro_created_date DESC;

-- 4. CEK SEMUA STATUS UNIK YANG ADA
SELECT DISTINCT dro_status
FROM sia_msdropout
ORDER BY dro_status;

-- 5. CEK DATA TERBARU (10 RECORD TERAKHIR) DENGAN SEMUA STATUS
SELECT TOP 10
    dro_id,
    mhs_id,
    dro_status,
    dro_created_by,
    dro_created_date
FROM sia_msdropout
ORDER BY dro_created_date DESC;

-- 6. CEK APAKAH KOLOM dro_status BISA MENYIMPAN "Revisi"
-- (Cek tipe data dan panjang kolom)
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'sia_msdropout'
  AND COLUMN_NAME = 'dro_status';

-- 7. CEK DISTRIBUSI STATUS PER BULAN (2026)
SELECT 
    YEAR(dro_created_date) as tahun,
    MONTH(dro_created_date) as bulan,
    dro_status,
    COUNT(*) as jumlah
FROM sia_msdropout
WHERE YEAR(dro_created_date) = 2026
GROUP BY YEAR(dro_created_date), MONTH(dro_created_date), dro_status
ORDER BY tahun DESC, bulan DESC, jumlah DESC;

-- 8. CEK APAKAH ADA DATA YANG PERNAH DIUBAH STATUSNYA
-- (Cek data yang punya modif_date)
SELECT 
    dro_id,
    mhs_id,
    dro_status,
    dro_created_date,
    dro_modif_date,
    dro_modif_by
FROM sia_msdropout
WHERE dro_modif_date IS NOT NULL
ORDER BY dro_modif_date DESC;

-- =============================================
-- QUERY UNTUK TEST/SIMULASI
-- =============================================

-- 9. SIMULASI: Update 1 data ke status "Revisi" untuk testing
-- ⚠️ JANGAN JALANKAN DI PRODUCTION TANPA BACKUP!
/*
UPDATE sia_msdropout
SET dro_status = 'Revisi',
    dro_modif_date = GETDATE(),
    dro_modif_by = 'admin_test'
WHERE dro_id = '4'  -- Ganti dengan ID yang mau di-test
  AND dro_status = 'Draft';  -- Hanya update yang masih Draft

-- Cek hasil update
SELECT * FROM sia_msdropout WHERE dro_id = '4';
*/

-- 10. ROLLBACK: Kembalikan ke status semula jika perlu
/*
UPDATE sia_msdropout
SET dro_status = 'Draft',
    dro_modif_date = GETDATE(),
    dro_modif_by = 'admin_rollback'
WHERE dro_id = '4';
*/

-- =============================================
-- EXPECTED RESULTS
-- =============================================

-- Jika TIDAK ADA data dengan status "Revisi":
-- Query #1 akan return: (0 rows affected)
-- Query #2 akan show: Draft, Belum Disetujui Wadir 1, Menunggu Upload SK, Disetujui
-- Query #4 akan show: Belum Disetujui Wadir 1, Disetujui, Draft, Menunggu Upload SK

-- Jika ADA data dengan status "Revisi":
-- Query #1 akan return: List data dengan status Revisi
-- Query #2 akan include: Revisi dengan jumlahnya
-- Query #4 akan include: Revisi dalam list

-- =============================================
-- NOTES
-- =============================================
-- 1. Jalankan query #1 terlebih dahulu untuk cek cepat
-- 2. Query #2 untuk lihat summary semua status
-- 3. Query #4 untuk lihat semua status unik yang ada
-- 4. Query #9 HANYA untuk testing, jangan di production!
