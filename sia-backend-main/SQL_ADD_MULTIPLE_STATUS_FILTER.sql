-- =============================================
-- ADD Multiple Status Filter Support (Simple Version - No Function)
-- Parameter @status bisa menerima multiple values separated by comma
-- Contoh: @status = 'Draft,Belum Disetujui Wadir 1,Revisi'
-- Menggunakan pattern matching dengan CHARINDEX (tidak perlu function)
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- UPDATE SP: sia_getDataRiwayatDO
-- Add support for multiple status filter (simple version)
-- =============================================
ALTER PROCEDURE [dbo].[sia_getDataRiwayatDO]
    @username VARCHAR(50),
    @keyword VARCHAR(MAX),
    @sort_by VARCHAR(100),
    @kon_id VARCHAR(50),
    @role_id VARCHAR(50),
    @display_name VARCHAR(100),
    @status VARCHAR(MAX) = '',     -- NEW: Support multiple status (comma-separated)
    @Page INT = NULL,
    @PageSize INT = NULL,
    @TotalRecords INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @viewBy NVARCHAR(MAX);
    DECLARE @statusFilter NVARCHAR(MAX) = '';
    DECLARE @str VARCHAR(MAX);
    DECLARE @countSQL NVARCHAR(MAX);
    
    -- CEK MAHASISWA DULU (PRIORITAS TERTINGGI)
    IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    BEGIN
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    ELSE
    BEGIN
        SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
        
        IF @str = '1' OR @str = '2' 
        BEGIN
            SET @viewBy = ' AND dro_status IN (''Menunggu Upload SK'', ''Disetujui'', ''Draft'', ''Belum Disetujui Wadir 1'', ''Revisi'')';
        END 
        ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
        BEGIN
            SET @viewBy = ' AND dro_status IN (''Menunggu Upload SK'', ''Disetujui'', ''Draft'', ''Belum Disetujui Wadir 1'', ''Revisi'')';
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
    
    -- Build status filter (simple version using pattern matching)
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        -- Tambahkan comma di awal dan akhir untuk pattern matching
        -- Contoh: 'Draft,Revisi' menjadi ',Draft,Revisi,'
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        
        -- Gunakan CHARINDEX untuk check apakah status ada dalam list
        -- Pattern: ',Draft,' atau ',Revisi,' dll
        SET @statusFilter = ' AND CHARINDEX('','' + dro_status + '','', ''' + @statusPattern + ''') > 0';
    END
    
    -- Get total count
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
    BEGIN
        SET @countSQL = '
        SELECT @TotalRecords = COUNT(*)
        FROM sia_msdropout a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)
        AND (a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR c.kon_singkatan LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') ' 
             + @viewBy + @statusFilter;
        
        EXEC sp_executesql @countSQL, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
    END
    
    -- Build main query
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
         OR srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') ' 
         + @viewBy + @statusFilter + '
    ORDER BY ' + @sort_by;
    
    -- Add pagination
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
    BEGIN
        DECLARE @Offset INT = (@Page - 1) * @PageSize;
        SET @SQL = @SQL + '
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(@PageSize AS VARCHAR) + ' ROWS ONLY';
    END
    
    SET @SQL = @SQL + ';';
    
    EXEC(@SQL);
END
GO

PRINT 'SP sia_getDataRiwayatDO updated with multiple status filter support (simple version)'
GO

-- =============================================
-- UPDATE SP: sia_getDataPendingDO
-- Add support for multiple status filter (simple version)
-- =============================================
ALTER PROCEDURE [dbo].[sia_getDataPendingDO]
    @username VARCHAR(50),
    @keyword VARCHAR(MAX),
    @sort_by VARCHAR(100),
    @kon_id VARCHAR(50),
    @role_id VARCHAR(50),
    @display_name VARCHAR(100),
    @status VARCHAR(MAX) = '',     -- NEW: Support multiple status (comma-separated)
    @Page INT = NULL,
    @PageSize INT = NULL,
    @TotalRecords INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @viewBy NVARCHAR(MAX);
    DECLARE @statusFilter NVARCHAR(MAX) = '';
    DECLARE @str VARCHAR(MAX);
    DECLARE @kryid VARCHAR(MAX);
    DECLARE @kons VARCHAR(MAX);
    DECLARE @countSQL NVARCHAR(MAX);
    
    -- Ambil struktur organisasi dan karyawan ID
    SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
    SELECT @kryid = kry_id FROM ess_mskaryawan WHERE kry_username = @username;
    SELECT @kons = kon_id FROM sia_mskonsentrasi WHERE kon_sekprodi = @kryid;
    
    -- Logic filter berdasarkan role/struktur untuk PENGAJUAN
    -- CEK MAHASISWA DULU (PRIORITAS TERTINGGI)
    IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    BEGIN
        -- Mahasiswa bisa lihat semua pengajuannya sendiri
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    -- Wadir 1 (str_main_id = '2') lihat yang perlu approval Wadir 1
    ELSE IF @str = '2' 
    BEGIN
        SET @viewBy = ' AND dro_status IN (''Belum Disetujui Wadir 1'', ''Draft'', ''Revisi'')';
    END 
    -- Direktur (str_main_id = '1') lihat yang perlu approval Direktur
    ELSE IF @str = '1' 
    BEGIN
        SET @viewBy = ' AND dro_status IN (''Belum Disetujui Direktur'', ''Belum Disetujui Wadir 1'', ''Draft'', ''Revisi'')';
    END
    -- Admin/Staff tertentu lihat semua
    ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
    BEGIN
        SET @viewBy = ' AND dro_status IN (''Draft'', ''Belum Disetujui Wadir 1'', ''Belum Disetujui Direktur'', ''Revisi'', ''Menunggu Upload SK'')';
    END
    -- Role tertentu lihat yang sudah disetujui
    ELSE IF @str = '27' OR @str = '23' OR @str = '28' 
    BEGIN
        SET @viewBy = ' AND dro_status IN (''Disetujui'', ''Menunggu Upload SK'')';
    END
    -- Mahasiswa via role
    ELSE IF @role_id = 'ROL23' 
    BEGIN
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    -- Sekprodi: Filter by konsentrasi yang dia handle
    ELSE 
    BEGIN
        SET @viewBy = ' AND c.kon_sekprodi = ''' + @kryid + '''';
    END;
    
    -- Build status filter (simple version using pattern matching)
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        SET @statusFilter = ' AND CHARINDEX('','' + dro_status + '','', ''' + @statusPattern + ''') > 0';
    END
    
    -- Get total count (untuk pagination info)
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
    BEGIN
        SET @countSQL = '
        SELECT @TotalRecords = COUNT(*)
        FROM sia_msdropout a,
             sia_msmahasiswa b,
             sia_mskonsentrasi c,
             sia_msprodi d
        WHERE c.pro_id = d.pro_id 
        AND a.mhs_id = b.mhs_id 
        AND b.kon_id = c.kon_id 
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END) 
        AND (a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
             OR srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') ' 
             + @viewBy + @statusFilter;
        
        EXEC sp_executesql @countSQL, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
    END
    
    -- Build dynamic SQL untuk get data PENGAJUAN
    SET @SQL = '
    SELECT 
        a.dro_id,
        b.mhs_id,
        b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
        pro_singkatan + '' ('' + kon_singkatan + '')'' AS kon_nama,
        CONVERT(VARCHAR(11), a.dro_created_date, 106) AS dro_created_date, 
        dro_created_by,
        srt_no,
        dro_status
    FROM sia_msdropout a,
         sia_msmahasiswa b,
         sia_mskonsentrasi c,
         sia_msprodi d
    WHERE c.pro_id = d.pro_id 
    AND a.mhs_id = b.mhs_id 
    AND b.kon_id = c.kon_id 
    AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END) 
    AND (a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
         OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
         OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
         OR srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') ' 
         + @viewBy + @statusFilter + '
    ORDER BY ' + @sort_by;
    
    -- Tambahkan OFFSET FETCH jika pagination digunakan
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
    BEGIN
        DECLARE @Offset INT = (@Page - 1) * @PageSize;
        SET @SQL = @SQL + '
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(@PageSize AS VARCHAR) + ' ROWS ONLY';
    END
    
    SET @SQL = @SQL + ';';
    
    EXEC(@SQL);
END
GO

PRINT 'SP sia_getDataPendingDO updated with multiple status filter support (simple version)'
GO

-- =============================================
-- TESTING Multiple Status Filter (Simple Version)
-- =============================================

-- Test 1: Test pagination dengan user nda_admin
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '3',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 10,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
    
-- Lihat total
SELECT @total AS TotalRecords;
GO

-- Test 2: Single status (backward compatible)
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = 'Draft',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Single status - Total: ' + CAST(@total AS VARCHAR);
GO

-- Test 3: Multiple status (comma-separated)
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = 'Draft,Belum Disetujui Wadir 1,Revisi',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Multiple status - Total: ' + CAST(@total AS VARCHAR);
GO

-- Test 4: Empty status (all data)
DECLARE @total INT;
EXEC sia_getDataRiwayatDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Empty status (all) - Total: ' + CAST(@total AS VARCHAR);
GO

-- Test 5: Test sia_getDataPendingDO dengan multiple status
DECLARE @total INT;
EXEC sia_getDataPendingDO 
    @username = 'admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = 'admin',
    @display_name = '',
    @status = 'Draft,Belum Disetujui Wadir 1',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Pending - Multiple status - Total: ' + CAST(@total AS VARCHAR);
GO

PRINT '=== MULTIPLE STATUS FILTER COMPLETE (SIMPLE VERSION) ==='
PRINT 'No function needed - using CHARINDEX pattern matching'
PRINT 'Usage examples:'
PRINT '  Single: @status = ''Draft'''
PRINT '  Multiple: @status = ''Draft,Belum Disetujui Wadir 1,Revisi'''
PRINT '  All: @status = '''''
PRINT ''
PRINT 'How it works:'
PRINT '  Input: ''Draft,Revisi'' becomes pattern '',Draft,Revisi,'''
PRINT '  Check: CHARINDEX('',Draft,'', '',Draft,Revisi,'') > 0 = TRUE'
PRINT '  Check: CHARINDEX('',Disetujui,'', '',Draft,Revisi,'') > 0 = FALSE'
GO
