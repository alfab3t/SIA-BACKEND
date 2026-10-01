-- =============================================
-- Fix: Robust Konsentrasi Filter for sia_getDataPendingDO
-- Problem: No results when filtering by konsentrasi ID
-- Solution: Add debug info and better error handling
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

ALTER PROCEDURE [dbo].[sia_getDataPendingDO]
    @username VARCHAR(50),
    @keyword VARCHAR(MAX),
    @sort_by VARCHAR(100),
    @kon_id VARCHAR(50),
    @role_id VARCHAR(50),
    @display_name VARCHAR(100),
    @status VARCHAR(MAX) = '',
    @Page INT = NULL,
    @PageSize INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @viewBy NVARCHAR(MAX);
    DECLARE @statusFilter NVARCHAR(MAX) = '';
    DECLARE @konsentrasiFilter NVARCHAR(MAX) = '';
    DECLARE @str VARCHAR(MAX);
    DECLARE @kryid VARCHAR(MAX);
    DECLARE @kons VARCHAR(MAX);

    -- Debug: Print input parameters
    PRINT 'DEBUG - Input Parameters:'
    PRINT '@username: ' + ISNULL(@username, 'NULL')
    PRINT '@kon_id: ' + ISNULL(@kon_id, 'NULL')
    PRINT '@role_id: ' + ISNULL(@role_id, 'NULL')

    -- Ambil struktur organisasi dan karyawan ID
    SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
    SELECT @kryid = kry_id FROM ess_mskaryawan WHERE kry_username = @username;
    SELECT @kons = kon_id FROM sia_mskonsentrasi WHERE kon_sekprodi = @kryid;

    PRINT 'DEBUG - User Info:'
    PRINT '@str: ' + ISNULL(@str, 'NULL')
    PRINT '@kryid: ' + ISNULL(@kryid, 'NULL')
    PRINT '@kons: ' + ISNULL(@kons, 'NULL')

    -- Logic filter berdasarkan role/struktur untuk PENGAJUAN
    IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    BEGIN
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    ELSE IF @str = '2' 
    BEGIN
        SET @viewBy = ' AND a.dro_status IN (''Belum Disetujui Wadir 1'', ''Draft'', ''Revisi'')';
    END 
    ELSE IF @str = '1' 
    BEGIN
        SET @viewBy = ' AND a.dro_status IN (''Belum Disetujui Direktur'', ''Belum Disetujui Wadir 1'', ''Draft'', ''Revisi'')';
    END
    ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
    BEGIN
        SET @viewBy = ' AND a.dro_status IN (''Draft'', ''Belum Disetujui Wadir 1'', ''Belum Disetujui Direktur'', ''Revisi'', ''Menunggu Upload SK'')';
    END
    ELSE IF @str = '27' OR @str = '23' OR @str = '28' 
    BEGIN
        SET @viewBy = ' AND a.dro_status IN (''Disetujui'', ''Menunggu Upload SK'')';
    END
    ELSE IF @role_id = 'ROL23' 
    BEGIN
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    ELSE 
    BEGIN
        SET @viewBy = ' AND c.kon_sekprodi = ''' + @kryid + '''';
    END;

    -- Build konsentrasi filter (IMPROVED)
    IF @kon_id IS NOT NULL AND @kon_id != ''
    BEGIN
        -- Try both ID and name matching for flexibility
        SET @konsentrasiFilter = ' AND (b.kon_id = ''' + @kon_id + ''' OR c.kon_nama = ''' + @kon_id + ''')';
        PRINT 'DEBUG - Konsentrasi Filter: ' + @konsentrasiFilter
    END
    ELSE
    BEGIN
        SET @konsentrasiFilter = '';
        PRINT 'DEBUG - No konsentrasi filter applied'
    END

    -- Build status filter
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        SET @statusFilter = ' AND CHARINDEX('','' + a.dro_status + '','', ''' + @statusPattern + ''') > 0';
    END

    -- Set default sorting jika kosong
    IF @sort_by IS NULL OR @sort_by = ''
    BEGIN
        SET @sort_by = 'CASE WHEN a.dro_status IN (''Draft'', ''Revisi'') THEN 0 ELSE 1 END, COALESCE(a.dro_modif_date, a.dro_created_date) DESC';
    END

    -- Build dynamic SQL dengan ROW_NUMBER pagination
    SET @SQL = '
    SELECT * FROM (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY ' + @sort_by + ') AS rownum,
            a.dro_id,
            b.mhs_id,
            b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
            d.pro_singkatan + '' ('' + c.kon_singkatan + '')'' AS kon_nama,
            CONVERT(VARCHAR(11), a.dro_created_date, 106) AS dro_created_date,
            a.dro_created_by,
            a.srt_no,
            a.dro_status,
            COUNT(*) OVER() AS [Count]
        FROM sia_msdropout a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE 1=1 '
          + @konsentrasiFilter + '
          AND (a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
               OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
               OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
               OR a.srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') '
          + @viewBy + @statusFilter + '
    ) res';

    -- Tambahkan pagination jika parameter ada
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
    BEGIN
        SET @SQL = @SQL + ' WHERE rownum BETWEEN ' + CAST((@Page - 1) * @PageSize + 1 AS VARCHAR) + ' AND ' + CAST((@Page * @PageSize) AS VARCHAR);
    END

    SET @SQL = @SQL + ';';

    PRINT 'DEBUG - Final SQL:'
    PRINT @SQL

    EXEC(@SQL);
END
GO

PRINT 'SP sia_getDataPendingDO updated with robust konsentrasi filter and debug info'
GO

-- =============================================
-- Test Script
-- =============================================

-- Test 1: Dengan ID konsentrasi
PRINT '=== TEST 1: With Konsentrasi ID ==='
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '11',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 5
GO

-- Test 2: Dengan nama konsentrasi
PRINT '=== TEST 2: With Konsentrasi Name ==='
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = 'Teknologi Rekayasa Pemeliharaan Alat Berat',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 5
GO

-- Test 3: Tanpa filter konsentrasi
PRINT '=== TEST 3: Without Konsentrasi Filter ==='
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 5
GO

PRINT 'All tests completed - Check debug output above'
GO