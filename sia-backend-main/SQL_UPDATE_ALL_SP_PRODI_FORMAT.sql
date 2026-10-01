-- =============================================
-- UPDATE ALL SP - Format Prodi
-- Menyamakan format prodi di semua SP Pengunduran Diri
-- Format: "Teknik Informatika" (nama lengkap tanpa jenjang)
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- 1. SP: sia_getListProdi
-- Digunakan di: GET /api/pengundurandiri/prodi/list
-- =============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Update: Hapus jenjang, hanya tampilkan nama prodi
ALTER PROCEDURE [dbo].[sia_getListProdi]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pro_id, 
        pro_nama  -- Tanpa jenjang
    FROM sia_msprodi
    WHERE pro_status = 'Aktif'
    ORDER BY pro_nama;
END
GO

PRINT '✓ SP sia_getListProdi updated (removed jenjang)'
GO

-- =============================================
-- 2. SP: sia_getListProdibySekprod
-- Digunakan di: GET /api/pengundurandiri/prodi
-- =============================================

ALTER PROCEDURE [dbo].[sia_getListProdibySekprod]
    @p1 varchar(max), @p2 varchar(max), @p3 varchar(max), @p4 varchar(max), @p5 varchar(max),
    @p6 varchar(max), @p7 varchar(max), @p8 varchar(max), @p9 varchar(max), @p10 varchar(max),
    @p11 varchar(max), @p12 varchar(max), @p13 varchar(max), @p14 varchar(max), @p15 varchar(max),
    @p16 varchar(max), @p17 varchar(max), @p18 varchar(max), @p19 varchar(max), @p20 varchar(max),
    @p21 varchar(max), @p22 varchar(max), @p23 varchar(max), @p24 varchar(max), @p25 varchar(max),
    @p26 varchar(max), @p27 varchar(max), @p28 varchar(max), @p29 varchar(max), @p30 varchar(max),
    @p31 varchar(max), @p32 varchar(max), @p33 varchar(max), @p34 varchar(max), @p35 varchar(max),
    @p36 varchar(max), @p37 varchar(max), @p38 varchar(max), @p39 varchar(max), @p40 varchar(max),
    @p41 varchar(max), @p42 varchar(max), @p43 varchar(max), @p44 varchar(max), @p45 varchar(max),
    @p46 varchar(max), @p47 varchar(max), @p48 varchar(max), @p49 varchar(max), @p50 varchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Format baru: "Teknik Informatika" (tanpa jenjang)
    SELECT 
        b.pro_id,
        b.pro_nama AS pro_nama  -- Tanpa jenjang
    FROM sia_mskonsentrasi a
    INNER JOIN sia_msprodi b ON a.pro_id = b.pro_id
    INNER JOIN ess_mskaryawan k ON a.kon_sekprodi = k.kry_nama_depan + ' ' + k.kry_nama_blkg
    WHERE k.kry_username = @p1
END
GO

PRINT '✓ SP sia_getListProdibySekprod updated'
GO

-- =============================================
-- 3. SP: sia_getDataPengunduranDiri
-- Digunakan di: GET /api/pengundurandiri
-- =============================================

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
            b.mhs_nama,  -- BARU: Nama mahasiswa
            a.pdi_approval_prodi_by AS approve_prodi, 
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            srt_no, 
            pdi_status AS status,
            a.pdi_created_by,
            d.pro_nama AS prodi_nama,  -- Tanpa jenjang
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE a.pdi_status != 'Dihapus' 
          AND a.mhs_id = @user_id
        ORDER BY a.pdi_created_date ASC;
    END
    ELSE
    BEGIN
        SELECT 
            a.pdi_id,
            (CASE WHEN CHARINDEX('PMA', a.pdi_id) > 0 THEN a.pdi_id ELSE 'DRAFT' END) AS id,
            a.mhs_id,
            b.mhs_nama,  -- BARU: Nama mahasiswa
            a.pdi_approval_prodi_by AS approve_prodi, 
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            COALESCE(a.pdi_app_prodi_date, a.pdi_app_dir1_date) AS tanggal_disetujui,
            srt_no, 
            pdi_status AS status, 
            a.pdi_created_by,
            d.pro_nama AS prodi_nama,  -- Tanpa jenjang
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE a.pdi_status != 'Dihapus'
          AND (a.pdi_status = @pdi_status OR a.pdi_created_by = @user_id)
        ORDER BY a.pdi_created_date ASC;
    END
END
GO

PRINT '✓ SP sia_getDataPengunduranDiri updated'
GO

-- =============================================
-- 2. SP: sia_getDataRiwayatPengunduranDiri
-- Digunakan di: GET /api/pengundurandiri/riwayat
-- =============================================

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
            d.pro_nama AS prodi_nama,  -- Tanpa jenjang
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
            d.pro_nama AS prodi_nama,  -- Tanpa jenjang
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
            d.pro_nama AS prodi_nama,  -- Tanpa jenjang
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
            d.pro_nama AS prodi_nama,  -- Tanpa jenjang
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

PRINT '✓ SP sia_getDataRiwayatPengunduranDiri updated'
GO

PRINT ''
PRINT '========================================='
PRINT 'ALL SP UPDATED SUCCESSFULLY!'
PRINT '========================================='
PRINT 'Updated SPs:'
PRINT '1. sia_getListProdi'
PRINT '2. sia_getListProdibySekprod'
PRINT '3. sia_getDataPengunduranDiri'
PRINT '4. sia_getDataRiwayatPengunduranDiri'
PRINT ''
PRINT 'Format baru:'
PRINT '- prodi_nama: "Teknik Informatika" (tanpa jenjang)'
PRINT '- konsentrasi: "SE", "DS", dll'
PRINT '========================================='
GO
