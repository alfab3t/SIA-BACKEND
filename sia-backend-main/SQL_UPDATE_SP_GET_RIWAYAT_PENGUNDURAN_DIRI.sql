-- =============================================
-- Update SP sia_getDataRiwayatPengunduranDiri
-- Menyamakan format nama prodi dengan sia_getListProdi
-- Format: "Teknik Informatika" (nama lengkap tanpa jenjang)
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sia_getDataRiwayatPengunduranDiri]
    @username VARCHAR(50),
    @pdi_status VARCHAR(50),
    @unused VARCHAR(50),
    @keyword VARCHAR(MAX),
    @order_by VARCHAR(100),
    @kon_id VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Mode 1: Status "Belum Disetujui Wadir 1" (untuk Wadir 1)
    IF (@pdi_status = 'Belum Disetujui Wadir 1')
    BEGIN
        SELECT 
            a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), a.pdi_app_dir1_date, 106) AS tanggal_disetujui,
            srt_no,
            mhs_nama,
            pdi_status AS status,
            -- Format baru: "Teknik Informatika" (tanpa jenjang)
            d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE (a.pdi_status = 'Menunggu Upload SK' OR a.pdi_status = 'Disetujui')
          AND (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
          AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY
            CASE WHEN @order_by != 'no asc' THEN '' ELSE a.pdi_id END ASC,
            CASE WHEN @order_by != 'no desc' THEN '' ELSE a.pdi_id END DESC,
            CASE WHEN @order_by != 'nim asc' THEN '' ELSE a.mhs_id END ASC,
            CASE WHEN @order_by != 'nim desc' THEN '' ELSE a.mhs_id END DESC,
            CASE WHEN @order_by != 'pdi_created_date asc' THEN '' ELSE a.pdi_created_date END ASC,
            CASE WHEN @order_by != 'pdi_created_date desc' THEN '' ELSE a.pdi_created_date END DESC;
    END

    -- Mode 2: Status "Belum Disetujui Prodi" (untuk Prodi/Sekprodi)
    ELSE IF (@pdi_status = 'Belum Disetujui Prodi')
    BEGIN
        DECLARE @kon INT;
        
        -- Ambil konsentrasi ID berdasarkan sekprodi
        SELECT @kon = kon_id 
        FROM sia_mskonsentrasi a
        INNER JOIN ess_mskaryawan b ON a.kon_sekprodi = RTRIM(kry_nama_depan + ' ' + kry_nama_blkg)
        WHERE kry_username = @username;

        SELECT 
            a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), a.pdi_app_prodi_date, 106) AS tanggal_disetujui,
            srt_no,
            mhs_nama,
            -- Format baru: "Teknik Informatika" (tanpa jenjang)
            d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi,
            pdi_status AS status
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE (a.pdi_status = 'Belum Disetujui Wadir 1' OR 
               a.pdi_status = 'Menunggu Upload SK' OR 
               a.pdi_status = 'Disetujui')
          AND (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
          AND b.kon_id = @kon
          AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY
            CASE WHEN @order_by != 'no asc' THEN '' ELSE a.pdi_id END ASC,
            CASE WHEN @order_by != 'no desc' THEN '' ELSE a.pdi_id END DESC,
            CASE WHEN @order_by != 'nim asc' THEN '' ELSE a.mhs_id END ASC,
            CASE WHEN @order_by != 'nim desc' THEN '' ELSE a.mhs_id END DESC,
            CASE WHEN @order_by != 'pdi_created_date asc' THEN '' ELSE a.pdi_created_date END ASC,
            CASE WHEN @order_by != 'pdi_created_date desc' THEN '' ELSE a.pdi_created_date END DESC;
    END

    -- Mode 3: Status "Menunggu Upload SK" (untuk Admin)
    ELSE IF (@pdi_status = 'Menunggu Upload SK')
    BEGIN
        SELECT 
            a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), c.srt_created_date, 106) AS tanggal_disetujui,
            a.srt_no,
            mhs_nama,
            -- Format baru: "Teknik Informatika" (tanpa jenjang)
            d.pro_nama AS prodi_nama,
            z.kon_singkatan AS konsentrasi,
            pdi_status AS status
        FROM sia_mspengundurandiri a
        LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
        WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
          AND a.pdi_status = 'Disetujui'
          AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY
            CASE WHEN @order_by != 'no asc' THEN '' ELSE a.pdi_id END ASC,
            CASE WHEN @order_by != 'no desc' THEN '' ELSE a.pdi_id END DESC,
            CASE WHEN @order_by != 'nim asc' THEN '' ELSE a.mhs_id END ASC,
            CASE WHEN @order_by != 'nim desc' THEN '' ELSE a.mhs_id END DESC,
            CASE WHEN @order_by != 'pdi_created_date asc' THEN '' ELSE a.pdi_created_date END ASC,
            CASE WHEN @order_by != 'pdi_created_date desc' THEN '' ELSE a.pdi_created_date END DESC;
    END

    -- Mode 4: Status lainnya (default)
    ELSE
    BEGIN
        SELECT 
            a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), c.srt_created_date, 106) AS tanggal_disetujui,
            a.srt_no,
            mhs_nama,
            -- Format baru: "Teknik Informatika" (tanpa jenjang)
            d.pro_nama AS prodi_nama,
            z.kon_singkatan AS konsentrasi,
            pdi_status AS status
        FROM sia_mspengundurandiri a
        LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
        WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' OR
               UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
          AND a.pdi_status = @pdi_status
          AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY
            CASE WHEN @order_by != 'no asc' THEN '' ELSE a.pdi_id END ASC,
            CASE WHEN @order_by != 'no desc' THEN '' ELSE a.pdi_id END DESC,
            CASE WHEN @order_by != 'nim asc' THEN '' ELSE a.mhs_id END ASC,
            CASE WHEN @order_by != 'nim desc' THEN '' ELSE a.mhs_id END DESC,
            CASE WHEN @order_by != 'pdi_created_date asc' THEN '' ELSE a.pdi_created_date END ASC,
            CASE WHEN @order_by != 'pdi_created_date desc' THEN '' ELSE a.pdi_created_date END DESC;
    END
END
GO

-- =============================================
-- PERUBAHAN:
-- =============================================
-- BEFORE:
-- pro_singkatan + ' (' + kon_singkatan + ')' AS kon_singkatan
-- Contoh: "TI (SE)"
--
-- AFTER:
-- d.pro_nama AS prodi_nama,
-- c.kon_singkatan AS konsentrasi
-- Contoh: 
--   prodi_nama: "Teknik Informatika" (tanpa D3/D4)
--   konsentrasi: "SE"
--
-- =============================================
-- IMPACT:
-- =============================================
-- 1. Response structure berubah dari 1 kolom jadi 2 kolom
-- 2. Frontend perlu update untuk handle 2 kolom terpisah
-- 3. Format lebih jelas dan informatif
-- 4. Jenjang (D3/D4) dihapus dari output
-- =============================================

PRINT '✓ SP sia_getDataRiwayatPengunduranDiri updated successfully'
PRINT '  Format: prodi_nama (tanpa jenjang) + konsentrasi'
GO
