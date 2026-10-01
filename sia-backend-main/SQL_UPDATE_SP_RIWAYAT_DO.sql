USE [ERP_PolmanAstra_NDA]
GO

/****** 
Object:  StoredProcedure [dbo].[sia_getDataRiwayatDO]
Script Date: 1/28/2026 3:54:36 PM 
Description: Update untuk menampilkan nama prodi lengkap (bukan singkatan)
******/

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
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    ELSE
    BEGIN
        -- Ambil struktur organisasi user
        SELECT @str = str_main_id 
        FROM ess_mskaryawan 
        WHERE kry_username = @username;

        -- Logic filter berdasarkan role/struktur
        IF @str = '1' OR @str = '2' 
        BEGIN
            SET @viewBy = ' AND (dro_status <> ''Draft'' OR dro_status <> ''Revisi'' OR dro_status = ''Menunggu Upload SK'')';
        END 
        ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
        BEGIN
            SET @viewBy = ' AND (dro_status = ''Menunggu Upload SK'' OR dro_status = ''Disetujui'' OR dro_status = ''Draft'' OR dro_status = ''Belum Disetujui Wadir 1'')';
        END 
        ELSE IF @str = '27' OR @str = '23' OR @str = '28' 
        BEGIN
            SET @viewBy = ' AND dro_status = ''Disetujui''';
        END 
        ELSE IF @role_id = 'ROL23' 
        BEGIN
            SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
        END 
        ELSE 
        BEGIN
            SET @viewBy = ' AND c.kon_sekprodi = ''' + @display_name + ''' ';
        END;
    END

    -- Build dynamic SQL
    -- PERUBAHAN: Ganti pro_singkatan dengan pro_nama untuk menampilkan nama lengkap prodi
    SET @SQL = '
    SELECT 
        a.dro_id,
        b.mhs_id,
        b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
        d.pro_nama + '' ('' + c.kon_singkatan + '')'' AS kon_nama,  -- CHANGED: pro_nama instead of pro_singkatan
        CONVERT(VARCHAR(11), a.dro_created_date, 106) AS dro_created_date, 
        a.dro_created_by,
        a.srt_no,
        a.dro_status
    FROM sia_msdropout a
    INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
    WHERE 
        b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)
        AND (
            a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
            OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
            OR c.kon_singkatan LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
            OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
            OR d.pro_nama LIKE ''%'' + ''' + @keyword + ''' + ''%''  -- ADDED: Search by pro_nama
            OR a.srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%''
        ) ' + @viewBy + '
    ORDER BY ' + @sort_by + ';';

    EXEC(@SQL);
END
GO

-- Test SP setelah update
PRINT 'Stored Procedure sia_getDataRiwayatDO berhasil diupdate!'
PRINT 'Perubahan:'
PRINT '1. Kolom kon_nama sekarang menampilkan: pro_nama + (kon_singkatan)'
PRINT '2. Contoh: "Teknik Produksi dan Proses Manufaktur (TPM)"'
PRINT '3. Ditambahkan search by pro_nama di keyword'
PRINT ''
PRINT 'Test dengan:'
PRINT 'EXEC sia_getDataRiwayatDO @username=''admin'', @keyword='''', @sort_by=''a.dro_created_date desc'', @kon_id='''', @role_id=''ROLE_ADMIN'', @display_name=''Administrator'''
GO
