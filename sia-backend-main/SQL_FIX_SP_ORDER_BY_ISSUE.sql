-- =============================================
-- FIX ORDER BY Issue in sia_getDataRiwayatPengunduranDiri
-- Problem: CASE statement returns empty string causing "constant expression" error
-- Solution: Use proper ORDER BY with CASE that returns column names or NULL
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
    @kon_id VARCHAR(50),
    @status VARCHAR(MAX) = '',     -- Support multiple status (comma-separated)
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
    
    -- Validate and set default order_by if empty or invalid
    IF @order_by IS NULL OR @order_by = ''
        SET @order_by = 'pdi_created_date desc';
    
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
        ORDER BY ';
        
        -- Add proper ORDER BY clause
        IF @order_by = 'no asc'
            SET @sql1 = @sql1 + 'a.pdi_id ASC';
        ELSE IF @order_by = 'no desc'
            SET @sql1 = @sql1 + 'a.pdi_id DESC';
        ELSE IF @order_by = 'nim asc'
            SET @sql1 = @sql1 + 'a.mhs_id ASC';
        ELSE IF @order_by = 'nim desc'
            SET @sql1 = @sql1 + 'a.mhs_id DESC';
        ELSE IF @order_by = 'pdi_created_date asc'
            SET @sql1 = @sql1 + 'a.pdi_created_date ASC';
        ELSE
            SET @sql1 = @sql1 + 'a.pdi_created_date DESC'; -- default
        
        SET @sql1 = @sql1 + '
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
            WHERE (a.pdi_status = ''Belum Disetujui Wadir 1'' OR 
                   a.pdi_status = ''Menunggu Upload SK'' OR 
                   a.pdi_status = ''Disetujui'')
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
        WHERE (a.pdi_status = ''Belum Disetujui Wadir 1'' OR 
               a.pdi_status = ''Menunggu Upload SK'' OR 
               a.pdi_status = ''Disetujui'')
        AND (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
             OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
             OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
        AND b.kon_id = ' + CAST(@kon AS VARCHAR) + '
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)' + @statusFilter + '
        ORDER BY ';
        
        -- Add proper ORDER BY clause
        IF @order_by = 'no asc'
            SET @sql2 = @sql2 + 'a.pdi_id ASC';
        ELSE IF @order_by = 'no desc'
            SET @sql2 = @sql2 + 'a.pdi_id DESC';
        ELSE IF @order_by = 'nim asc'
            SET @sql2 = @sql2 + 'a.mhs_id ASC';
        ELSE IF @order_by = 'nim desc'
            SET @sql2 = @sql2 + 'a.mhs_id DESC';
        ELSE IF @order_by = 'pdi_created_date asc'
            SET @sql2 = @sql2 + 'a.pdi_created_date ASC';
        ELSE
            SET @sql2 = @sql2 + 'a.pdi_created_date DESC'; -- default
        
        SET @sql2 = @sql2 + '
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
        ORDER BY ';
        
        -- Add proper ORDER BY clause
        IF @order_by = 'no asc'
            SET @sql3 = @sql3 + 'a.pdi_id ASC';
        ELSE IF @order_by = 'no desc'
            SET @sql3 = @sql3 + 'a.pdi_id DESC';
        ELSE IF @order_by = 'nim asc'
            SET @sql3 = @sql3 + 'a.mhs_id ASC';
        ELSE IF @order_by = 'nim desc'
            SET @sql3 = @sql3 + 'a.mhs_id DESC';
        ELSE IF @order_by = 'pdi_created_date asc'
            SET @sql3 = @sql3 + 'a.pdi_created_date ASC';
        ELSE
            SET @sql3 = @sql3 + 'a.pdi_created_date DESC'; -- default
        
        SET @sql3 = @sql3 + '
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
        ORDER BY ';
        
        -- Add proper ORDER BY clause
        IF @order_by = 'no asc'
            SET @sql4 = @sql4 + 'a.pdi_id ASC';
        ELSE IF @order_by = 'no desc'
            SET @sql4 = @sql4 + 'a.pdi_id DESC';
        ELSE IF @order_by = 'nim asc'
            SET @sql4 = @sql4 + 'a.mhs_id ASC';
        ELSE IF @order_by = 'nim desc'
            SET @sql4 = @sql4 + 'a.mhs_id DESC';
        ELSE IF @order_by = 'pdi_created_date asc'
            SET @sql4 = @sql4 + 'a.pdi_created_date ASC';
        ELSE
            SET @sql4 = @sql4 + 'a.pdi_created_date DESC'; -- default
        
        SET @sql4 = @sql4 + '
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql4);
    END
END
GO

PRINT '✅ SP sia_getDataRiwayatPengunduranDiri ORDER BY issue fixed!'
PRINT '   - Replaced problematic CASE statements with IF-ELSE logic'
PRINT '   - Added proper default handling for empty order_by parameter'
PRINT '   - Maintained pagination and multiple status filter support'
GO

-- Test the fixed SP
PRINT 'Testing fixed SP...'
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'nda_admin',
    @pdi_status = '',
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
    
PRINT 'Test completed. Total records: ' + CAST(@total AS VARCHAR);
GO