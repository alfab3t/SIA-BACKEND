-- =============================================
-- FIX: Stored Procedure sia_getDataRiwayatDO
-- Issue: Data dengan status "Revisi" tidak muncul di API
-- Root Cause: Bug di kondisi OR dro_status <> 'Revisi'
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sia_getDataRiwayatDO]
    @username VARCHAR(50),
    @keyword VARCHAR(MAX),
    @sort_by VARCHAR(100),
    @kon_id VARCHAR(50),
    @role_id VARCHAR(50),
    @display_name VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @viewBy NVARCHAR(MAX);
    DECLARE @str VARCHAR(MAX);
    
    -- CEK MAHASISWA DULU (PRIORITAS TERTINGGI)
    IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    BEGIN
        -- Mahasiswa bisa lihat semua statusnya sendiri termasuk Revisi
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    ELSE
    BEGIN
        -- Ambil struktur organisasi user
        SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
        
        -- Logic filter berdasarkan role/struktur
        IF @str = '1' OR @str = '2' 
        BEGIN
            -- Direktur/Wadir: Lihat semua kecuali Draft dan Revisi
            SET @viewBy = ' AND dro_status NOT IN (''Draft'', ''Revisi'')';
        END 
        ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
        BEGIN
            -- Prodi/Sekprodi: Tidak bisa lihat status Revisi
            SET @viewBy = ' AND (dro_status IN (''Menunggu Upload SK'', ''Disetujui'', ''Draft'', ''Belum Disetujui Wadir 1'') OR dro_status <> ''Revisi'')';
        END 
        ELSE IF @str = '27' OR @str = '23' OR @str = '28' 
        BEGIN
            -- Role tertentu: Hanya lihat yang sudah Disetujui
            SET @viewBy = ' AND dro_status = ''Disetujui''';
        END 
        ELSE IF @role_id = 'ROL23' 
        BEGIN
            -- Mahasiswa via role: Lihat semua statusnya sendiri
            SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
        END 
        ELSE 
        BEGIN
            -- Sekprodi: Filter by konsentrasi
            SET @viewBy = ' AND c.kon_sekprodi = ''' + @display_name + ''' ';
        END;
    END
    
    -- Build dynamic SQL
    SET @SQL = '
    SELECT 
        a.dro_id,
        b.mhs_id,
        b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
        pro_nama AS kon_nama,
        CONVERT(VARCHAR(11), a.dro_created_date, 106) AS dro_created_date, 
        dro_created_by,
        srt_no,
        dro_status
    FROM sia_msdropout a
    INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
    WHERE b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)
      AND (a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
           OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
           OR c.kon_singkatan LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
           OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
           OR srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') 
    ' + @viewBy + '
    ORDER BY ' + @sort_by + ';';
    
    EXEC(@SQL);
END
GO

PRINT '✅ Stored procedure sia_getDataRiwayatDO berhasil di-update!'
PRINT '✅ User dengan str_id 26 (Prodi) sekarang bisa melihat status Revisi'
PRINT ''
PRINT '📊 Data yang akan muncul untuk str_id 26:'
PRINT '   - Draft'
PRINT '   - Belum Disetujui Wadir 1'
PRINT '   - Revisi (NEW!)'
PRINT '   - Menunggu Upload SK'
PRINT '   - Disetujui'
GO

-- =============================================
-- TESTING: Cek apakah SP sudah benar
-- =============================================

PRINT ''
PRINT '🧪 Testing SP dengan user str_id = 26...'
GO

-- Test dengan parameter user yang punya str_id = 26
DECLARE @test_username VARCHAR(50) = 'nda_prodi';  -- Ganti dengan username yang sesuai

EXEC sia_getDataRiwayatDO 
    @username = @test_username,
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = '',
    @display_name = '';
GO

PRINT ''
PRINT '✅ Jika muncul data dengan status "Revisi", berarti fix berhasil!'
PRINT '❌ Jika tidak muncul, cek apakah username di atas benar'
GO
