-- =============================================
-- Update SP sia_getDataPengunduranDiri
-- Menambahkan kolom prodi_nama dan konsentrasi
-- Format: "Teknik Informatika" (tanpa jenjang) + "SE"
-- 
-- PERUBAHAN:
-- 1. Tambah JOIN ke sia_mskonsentrasi (alias: c)
-- 2. Tambah JOIN ke sia_msprodi (alias: d)
-- 3. Tambah kolom prodi_nama dan konsentrasi di SELECT
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
            -- BARU: Tambahan kolom prodi dan konsentrasi
            d.pro_jenjang + ' ' + d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        -- BARU: JOIN ke konsentrasi dan prodi
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
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
            -- BARU: Tambahan kolom prodi dan konsentrasi
            d.pro_jenjang + ' ' + d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        -- BARU: JOIN ke konsentrasi dan prodi
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE a.pdi_status != 'Dihapus'
          AND (a.pdi_status = @pdi_status OR a.pdi_created_by = @user_id)
        ORDER BY a.pdi_created_date ASC;
    END
END
GO

-- =============================================
-- SUMMARY PERUBAHAN:
-- =============================================
-- BEFORE (SP Asli):
-- - Hanya JOIN ke sia_msmahasiswa
-- - Tidak ada kolom prodi/konsentrasi
--
-- AFTER (SP Baru):
-- - JOIN ke sia_msmahasiswa (alias: b)
-- - JOIN ke sia_mskonsentrasi (alias: c) ← BARU
-- - JOIN ke sia_msprodi (alias: d) ← BARU
-- - Tambah kolom: prodi_nama ← BARU
-- - Tambah kolom: konsentrasi ← BARU
--
-- Output baru:
--   prodi_nama: "Teknik Informatika"
--   konsentrasi: "SE"
-- =============================================
