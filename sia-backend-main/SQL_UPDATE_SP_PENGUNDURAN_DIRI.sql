-- =============================================
-- Update SP: sia_getDataPengunduranDiri
-- Tujuan: Menambahkan nama prodi lengkap (bukan singkatan)
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

ALTER PROCEDURE [dbo].[sia_getDataPengunduranDiri]
@user_id VARCHAR(50),
@pdi_status VARCHAR(50),
@kry_id VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Mode 1: Mahasiswa melihat data sendiri (status kosong)
    IF (@pdi_status = '')
    BEGIN
        SELECT 
            pdi_id,
            (CASE WHEN CHARINDEX('PMA', a.pdi_id) > 0 THEN a.pdi_id ELSE 'Draft' END) AS id,
            a.mhs_id, 
            a.pdi_approval_prodi_by AS approve_prodi, 
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            srt_no, 
            pdi_status AS status,
            a.pdi_created_by,
            -- ✅ TAMBAHAN: Nama mahasiswa dan prodi lengkap
            b.mhs_nama,
            d.pro_nama + ' (' + c.kon_singkatan + ')' AS prodi_nama  -- Nama prodi lengkap
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE a.pdi_status != 'Dihapus' 
          AND a.mhs_id = @user_id
        ORDER BY a.pdi_created_date ASC;
    END
    ELSE
    BEGIN
        -- Mode 2: Admin/Staff melihat berdasarkan status atau data yang dibuat sendiri
        SELECT 
            a.pdi_id,
            (CASE WHEN CHARINDEX('PMA', a.pdi_id) > 0 THEN a.pdi_id ELSE 'DRAFT' END) AS id,
            a.mhs_id, 
            a.pdi_approval_prodi_by AS approve_prodi, 
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            COALESCE(a.pdi_app_prodi_date, a.pdi_app_dir1_date) AS tanggal_disetujui,
            srt_no, 
            pdi_status AS status, 
            a.pdi_created_by,
            -- ✅ TAMBAHAN: Nama mahasiswa dan prodi lengkap
            b.mhs_nama,
            d.pro_nama + ' (' + c.kon_singkatan + ')' AS prodi_nama  -- Nama prodi lengkap
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE a.pdi_status != 'Dihapus'
          AND (a.pdi_status = @pdi_status OR a.pdi_created_by = @user_id)
        ORDER BY a.pdi_created_date ASC;
    END
END
GO

-- =============================================
-- Testing Query
-- =============================================

-- Test Mode 1: Mahasiswa
-- EXEC sia_getDataPengunduranDiri @user_id = '123456', @pdi_status = '', @kry_id = ''

-- Test Mode 2: Admin/Staff
-- EXEC sia_getDataPengunduranDiri @user_id = 'admin', @pdi_status = 'Belum Disetujui Prodi', @kry_id = ''

-- =============================================
-- Expected Output
-- =============================================
-- Kolom baru yang ditambahkan:
-- 1. mhs_nama          : Nama mahasiswa
-- 2. prodi_nama        : Nama prodi lengkap + konsentrasi
--                        Contoh: "Teknik Informatika (SE)"
--                                "Teknik Mesin (MFG)"
-- =============================================
