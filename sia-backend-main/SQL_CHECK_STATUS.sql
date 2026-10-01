-- =============================================
-- Query untuk Cek Status di Tabel Pengunduran Diri
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- 1. Cek DISTINCT Status (Unique)
-- =============================================
SELECT DISTINCT pdi_status AS status
FROM sia_mspengundurandiri
WHERE pdi_status IS NOT NULL
ORDER BY pdi_status;

-- =============================================
-- 2. Cek Status dengan Jumlah Data
-- =============================================
SELECT 
    pdi_status AS status,
    COUNT(*) AS jumlah_data
FROM sia_mspengundurandiri
WHERE pdi_status IS NOT NULL
GROUP BY pdi_status
ORDER BY jumlah_data DESC;

-- =============================================
-- 3. Cek Status dengan Detail Tambahan
-- =============================================
SELECT 
    pdi_status AS status,
    COUNT(*) AS jumlah_data,
    MIN(pdi_created_date) AS tanggal_pertama,
    MAX(pdi_created_date) AS tanggal_terakhir
FROM sia_mspengundurandiri
WHERE pdi_status IS NOT NULL
GROUP BY pdi_status
ORDER BY pdi_status;

-- =============================================
-- 4. Cek Status dengan Persentase
-- =============================================
SELECT 
    pdi_status AS status,
    COUNT(*) AS jumlah_data,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM sia_mspengundurandiri WHERE pdi_status IS NOT NULL) AS DECIMAL(5,2)) AS persentase
FROM sia_mspengundurandiri
WHERE pdi_status IS NOT NULL
GROUP BY pdi_status
ORDER BY jumlah_data DESC;

-- =============================================
-- 5. Cek Status NULL atau Empty
-- =============================================
SELECT 
    'NULL atau Empty' AS kategori,
    COUNT(*) AS jumlah_data
FROM sia_mspengundurandiri
WHERE pdi_status IS NULL OR pdi_status = '';

-- =============================================
-- 6. Cek Sample Data per Status
-- =============================================
SELECT 
    pdi_status AS status,
    pdi_id,
    mhs_id,
    pdi_created_by,
    CONVERT(VARCHAR(20), pdi_created_date, 120) AS created_date
FROM sia_mspengundurandiri
WHERE pdi_status IS NOT NULL
ORDER BY pdi_status, pdi_created_date DESC;

-- =============================================
-- 7. Cek Status Flow (Urutan Status)
-- =============================================
-- Untuk melihat urutan status yang paling sering terjadi
SELECT 
    pdi_id,
    mhs_id,
    pdi_status,
    pdi_approval_prodi_by,
    pdi_approval_dir1_by,
    CONVERT(VARCHAR(20), pdi_created_date, 120) AS created_date,
    CONVERT(VARCHAR(20), pdi_app_prodi_date, 120) AS approved_prodi_date,
    CONVERT(VARCHAR(20), pdi_app_dir1_date, 120) AS approved_dir1_date
FROM sia_mspengundurandiri
WHERE pdi_status IS NOT NULL
ORDER BY pdi_created_date DESC;

-- =============================================
-- Expected Status List (Berdasarkan Business Logic)
-- =============================================
/*
Status yang seharusnya ada:
1. Draft                        - Baru dibuat, belum diajukan
2. Belum Disetujui Prodi        - Menunggu approval Prodi
3. Ditolak Prodi                - Ditolak oleh Prodi
4. Belum Disetujui Wadir 1      - Menunggu approval Wadir 1
5. Ditolak Wadir 1              - Ditolak oleh Wadir 1
6. Menunggu Upload SK           - Disetujui, menunggu upload SK
7. Disetujui                    - Sudah upload SK, selesai
8. Dihapus                      - Soft delete

Status Flow:
Draft → Belum Disetujui Prodi → Belum Disetujui Wadir 1 → Menunggu Upload SK → Disetujui
         ↓                              ↓
    Ditolak Prodi              Ditolak Wadir 1
*/
