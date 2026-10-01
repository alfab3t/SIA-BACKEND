-- =============================================
-- ADD Multiple Status Filter to Pengunduran Diri SPs
-- Menambahkan parameter @status untuk filter multiple status (comma-separated)
-- Menggunakan CHARINDEX pattern matching (tanpa fungsi tambahan)
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- UPDATE SP: sia_getDataPengunduranDiri
-- Add @status parameter with multiple status support
-- =============================================
ALTER PROCEDURE [dbo].[sia_getDataPengunduranDiri]
    @user_id VARCHAR(50),
    @pdi_status VARCHAR(50),
    @kry_id VARCHAR(50),
    @status VARCHAR(MAX) = '',     -- NEW: Support multiple status (comma-separated)
    @Page INT = NULL,
    @PageSize INT = NULL,
    @TotalRecords INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = 0;
    DECLARE @statusFilter NVARCHAR(MAX) = '';
    
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        SET @Offset = (@Page - 1) * @PageSize;
    
    -- Build status filter (simple version using pattern matching)
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        SET @statusFilter = ' AND CHARINDEX('','' + pdi_status + '','', ''' + @statusPattern + ''') > 0';
    END
    
    -- Mode 1: Mahasiswa melihat data sendiri (status kosong)
    IF (@pdi_status = '')
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL1 NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
            INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
            WHERE a.pdi_status != ''Dihapus'' AND a.mhs_id = ''' + @user_id + '''' + @statusFilter;
            
            EXEC sp_executesql @countSQL1, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql1 NVARCHAR(MAX) = '
        SELECT pdi_id,
            (CASE WHEN CHARINDEX(''PMA'', a.pdi_id) > 0 THEN a.pdi_id ELSE ''Draft'' END) AS id,
            a.mhs_id,
            b.mhs_nama,
            a.pdi_approval_prodi_by AS approve_prodi, 
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            srt_no, 
            pdi_status AS status,
            a.pdi_created_by,
            d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE a.pdi_status != ''Dihapus'' AND a.mhs_id = ''' + @user_id + '''' + @statusFilter + '
        ORDER BY a.pdi_created_date ASC
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql1);
    END
    ELSE
    BEGIN
        -- Mode 2: Admin/Staff melihat berdasarkan status atau data yang dibuat sendiri
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL2 NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
            INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
            WHERE a.pdi_status != ''Dihapus''
            AND (a.pdi_status = ''' + @pdi_status + ''' OR a.pdi_created_by = ''' + @user_id + ''')' + @statusFilter;
            
            EXEC sp_executesql @countSQL2, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql2 NVARCHAR(MAX) = '
        SELECT a.pdi_id,
            (CASE WHEN CHARINDEX(''PMA'', a.pdi_id) > 0 THEN a.pdi_id ELSE ''DRAFT'' END) AS id,
            a.mhs_id,
            b.mhs_nama,
            a.pdi_approval_prodi_by AS approve_prodi, 
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            COALESCE(a.pdi_app_prodi_date, a.pdi_app_dir1_date) AS tanggal_disetujui,
            srt_no, 
            pdi_status AS status, 
            a.pdi_created_by,
            d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
        WHERE a.pdi_status != ''Dihapus''
        AND (a.pdi_status = ''' + @pdi_status + ''' OR a.pdi_created_by = ''' + @user_id + ''')' + @statusFilter + '
        ORDER BY a.pdi_created_date ASC
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql2);
    END
END
GO

PRINT 'SP sia_getDataPengunduranDiri updated with multiple status filter support'
GO

-- =============================================
-- UPDATE SP: sia_getDataRiwayatPengunduranDiri
-- Add @status parameter with multiple status support
-- =============================================
ALTER PROCEDURE [dbo].[sia_getDataRiwayatPengunduranDiri]
    @username VARCHAR(50),
    @pdi_status VARCHAR(50),
    @unused VARCHAR(50),
    @keyword VARCHAR(MAX),
    @order_by VARCHAR(100),
    @kon_id VARCHAR(50),
    @status VARCHAR(MAX) = '',     -- NEW: Support multiple status (comma-separated)
    @Page INT = NULL,
    @PageSize INT = NULL,
    @TotalRecords INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = 0;
    DECLARE @statusFilter NVARCHAR(MAX) = '';
    
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        SET @Offset = (@Page - 1) * @PageSize;
    
    -- Build status filter (simple version using pattern matching)
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        SET @statusFilter = ' AND CHARINDEX('','' + pdi_status + '','', ''' + @statusPattern + ''') > 0';
    END
    
    -- Mode 1: Status "Belum Disetujui Wadir 1" (untuk Wadir 1)
    IF (@pdi_status = 'Belum Disetujui Wadir 1')
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL1 NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
            WHERE (a.pdi_status = ''Menunggu Upload SK'' OR a.pdi_status = ''Disetujui'')
            AND (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                 OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                 OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
            AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter;
            
            EXEC sp_executesql @countSQL1, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql1 NVARCHAR(MAX) = '
        SELECT a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), a.pdi_app_dir1_date, 106) AS tanggal_disetujui,
            srt_no,
            mhs_nama,
            pdi_status AS status,
            d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE (a.pdi_status = ''Menunggu Upload SK'' OR a.pdi_status = ''Disetujui'')
        AND (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
             OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
             OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter + '
        ORDER BY
            CASE WHEN ''' + @order_by + ''' != ''no asc'' THEN '''' ELSE a.pdi_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''no desc'' THEN '''' ELSE a.pdi_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''nim asc'' THEN '''' ELSE a.mhs_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''nim desc'' THEN '''' ELSE a.mhs_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date asc'' THEN '''' ELSE a.pdi_created_date END ASC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date desc'' THEN '''' ELSE a.pdi_created_date END DESC
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql1);
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
        
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL2 NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
            WHERE (a.pdi_status = ''Belum Disetujui Wadir 1'' OR a.pdi_status = ''Menunggu Upload SK'' OR a.pdi_status = ''Disetujui'')
            AND (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                 OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                 OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
            AND b.kon_id = ' + CAST(@kon AS VARCHAR) + '
            AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter;
            
            EXEC sp_executesql @countSQL2, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql2 NVARCHAR(MAX) = '
        SELECT a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), a.pdi_app_prodi_date, 106) AS tanggal_disetujui,
            srt_no,
            mhs_nama,
            d.pro_nama AS prodi_nama,
            c.kon_singkatan AS konsentrasi,
            pdi_status AS status
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE (a.pdi_status = ''Belum Disetujui Wadir 1'' OR a.pdi_status = ''Menunggu Upload SK'' OR a.pdi_status = ''Disetujui'')
        AND (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
             OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
             OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
        AND b.kon_id = ' + CAST(@kon AS VARCHAR) + '
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter + '
        ORDER BY
            CASE WHEN ''' + @order_by + ''' != ''no asc'' THEN '''' ELSE a.pdi_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''no desc'' THEN '''' ELSE a.pdi_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''nim asc'' THEN '''' ELSE a.mhs_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''nim desc'' THEN '''' ELSE a.mhs_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date asc'' THEN '''' ELSE a.pdi_created_date END ASC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date desc'' THEN '''' ELSE a.pdi_created_date END DESC
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql2);
    END
    -- Mode 3: Status "Menunggu Upload SK" (untuk Admin)
    ELSE IF (@pdi_status = 'Menunggu Upload SK')
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL3 NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
            WHERE (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                   OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                   OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
            AND a.pdi_status = ''Disetujui''
            AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter;
            
            EXEC sp_executesql @countSQL3, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql3 NVARCHAR(MAX) = '
        SELECT a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), c.srt_created_date, 106) AS tanggal_disetujui,
            a.srt_no,
            mhs_nama,
            d.pro_nama AS prodi_nama,
            z.kon_singkatan AS konsentrasi,
            pdi_status AS status
        FROM sia_mspengundurandiri a
        LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
        WHERE (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
               OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
               OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
        AND a.pdi_status = ''Disetujui''
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter + '
        ORDER BY
            CASE WHEN ''' + @order_by + ''' != ''no asc'' THEN '''' ELSE a.pdi_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''no desc'' THEN '''' ELSE a.pdi_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''nim asc'' THEN '''' ELSE a.mhs_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''nim desc'' THEN '''' ELSE a.mhs_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date asc'' THEN '''' ELSE a.pdi_created_date END ASC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date desc'' THEN '''' ELSE a.pdi_created_date END DESC
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql3);
    END
    -- Mode 4: Status lainnya (default)
    ELSE
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL4 NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
            WHERE (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                   OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                   OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
            AND a.pdi_status = ''' + @pdi_status + '''
            AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter;
            
            EXEC sp_executesql @countSQL4, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql4 NVARCHAR(MAX) = '
        SELECT a.pdi_id,
            a.mhs_id,
            a.pdi_approval_prodi_by AS approve_prodi,
            a.pdi_approval_dir1_by AS approve_dir1,
            CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
            CONVERT(VARCHAR(11), c.srt_created_date, 106) AS tanggal_disetujui,
            a.srt_no,
            mhs_nama,
            d.pro_nama AS prodi_nama,
            z.kon_singkatan AS konsentrasi,
            pdi_status AS status
        FROM sia_mspengundurandiri a
        LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
        WHERE (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
               OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
               OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
        AND a.pdi_status = ''' + @pdi_status + '''
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter + '
        ORDER BY
            CASE WHEN ''' + @order_by + ''' != ''no asc'' THEN '''' ELSE a.pdi_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''no desc'' THEN '''' ELSE a.pdi_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''nim asc'' THEN '''' ELSE a.mhs_id END ASC,
            CASE WHEN ''' + @order_by + ''' != ''nim desc'' THEN '''' ELSE a.mhs_id END DESC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date asc'' THEN '''' ELSE a.pdi_created_date END ASC,
            CASE WHEN ''' + @order_by + ''' != ''pdi_created_date desc'' THEN '''' ELSE a.pdi_created_date END DESC
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql4);
    END
END
GO

PRINT 'SP sia_getDataRiwayatPengunduranDiri updated with multiple status filter support'
GO

-- =============================================
-- TESTING Multiple Status Filter for Pengunduran Diri
-- =============================================

-- Test 1: Single status
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'admin',
    @pdi_status = '',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @status = 'Draft',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Single status - Total: ' + CAST(@total AS VARCHAR);
GO

-- Test 2: Multiple status
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'admin',
    @pdi_status = '',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @status = 'Draft,Belum Disetujui Wadir 1,Disetujui',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Multiple status - Total: ' + CAST(@total AS VARCHAR);
GO

-- Test 3: Empty status (all data)
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'admin',
    @pdi_status = '',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
PRINT 'Empty status (all) - Total: ' + CAST(@total AS VARCHAR);
GO

PRINT '=== MULTIPLE STATUS FILTER FOR PENGUNDURAN DIRI COMPLETE ==='
PRINT 'No function needed - using CHARINDEX pattern matching'
PRINT 'Usage examples:'
PRINT '  Single: @status = ''Draft'''
PRINT '  Multiple: @status = ''Draft,Belum Disetujui Wadir 1,Disetujui'''
PRINT '  All: @status = '''''
GO
