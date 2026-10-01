-- =============================================
-- SIMPLE FIX for Mode 4 Multiple Status Issue
-- Replace the complex CHARINDEX logic with simple IN clause
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
    
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        SET @Offset = (@Page - 1) * @PageSize;
    
    -- Validate and set default order_by if empty or invalid
    IF @order_by IS NULL OR @order_by = ''
        SET @order_by = 'pdi_created_date desc';
    
    -- Mode 1: Status "Belum Disetujui Wadir 1" (untuk Wadir 1)
    IF (@pdi_status = 'Belum Disetujui Wadir 1')
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
            WHERE (a.pdi_status = 'Menunggu Upload SK' OR a.pdi_status = 'Disetujui')
            AND (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' 
                 OR UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' 
                 OR UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
            AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END);
        END
        
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
        WHERE (a.pdi_status = 'Menunggu Upload SK' OR a.pdi_status = 'Disetujui')
        AND (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' 
             OR UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' 
             OR UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
        AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY 
            CASE WHEN @order_by = 'no asc' THEN a.pdi_id END ASC,
            CASE WHEN @order_by = 'no desc' THEN a.pdi_id END DESC,
            CASE WHEN @order_by = 'nim asc' THEN a.mhs_id END ASC,
            CASE WHEN @order_by = 'nim desc' THEN a.mhs_id END DESC,
            CASE WHEN @order_by = 'pdi_created_date asc' THEN a.pdi_created_date END ASC,
            a.pdi_created_date DESC -- default
        OFFSET @Offset ROWS
        FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
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
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
            WHERE (a.pdi_status = 'Belum Disetujui Wadir 1' OR 
                   a.pdi_status = 'Menunggu Upload SK' OR 
                   a.pdi_status = 'Disetujui')
            AND (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' 
                 OR UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' 
                 OR UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
            AND b.kon_id = @kon
            AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END);
        END
        
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
        WHERE (a.pdi_status = 'Belum Disetujui Wadir 1' OR 
               a.pdi_status = 'Menunggu Upload SK' OR 
               a.pdi_status = 'Disetujui')
        AND (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' 
             OR UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' 
             OR UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
        AND b.kon_id = @kon
        AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY 
            CASE WHEN @order_by = 'no asc' THEN a.pdi_id END ASC,
            CASE WHEN @order_by = 'no desc' THEN a.pdi_id END DESC,
            CASE WHEN @order_by = 'nim asc' THEN a.mhs_id END ASC,
            CASE WHEN @order_by = 'nim desc' THEN a.mhs_id END DESC,
            CASE WHEN @order_by = 'pdi_created_date asc' THEN a.pdi_created_date END ASC,
            a.pdi_created_date DESC -- default
        OFFSET @Offset ROWS
        FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
    END
    
    -- Mode 3: Status "Menunggu Upload SK" (untuk Admin)
    ELSE IF (@pdi_status = 'Menunggu Upload SK')
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
            WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' 
                   OR UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' 
                   OR UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
            AND a.pdi_status = 'Disetujui'
            AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END);
        END
        
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
        WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER(@keyword) + '%' 
               OR UPPER(b.mhs_nama) LIKE '%' + UPPER(@keyword) + '%' 
               OR UPPER(a.srt_no) LIKE '%' + UPPER(@keyword) + '%')
        AND a.pdi_status = 'Disetujui'
        AND b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
        ORDER BY 
            CASE WHEN @order_by = 'no asc' THEN a.pdi_id END ASC,
            CASE WHEN @order_by = 'no desc' THEN a.pdi_id END DESC,
            CASE WHEN @order_by = 'nim asc' THEN a.mhs_id END ASC,
            CASE WHEN @order_by = 'nim desc' THEN a.mhs_id END DESC,
            CASE WHEN @order_by = 'pdi_created_date asc' THEN a.pdi_created_date END ASC,
            a.pdi_created_date DESC -- default
        OFFSET @Offset ROWS
        FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
    END
    
    -- Mode 4: Status lainnya (default) - SIMPLE FIX untuk handle multiple status
    ELSE
    BEGIN
        -- Get total count untuk pagination
        IF @Page IS NOT NULL AND @PageSize IS NOT NULL
        BEGIN
            DECLARE @countSQL NVARCHAR(MAX) = '
            SELECT @TotalRecords = COUNT(*)
            FROM sia_mspengundurandiri a
            LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
            INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
            INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
            INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
            WHERE (UPPER(a.mhs_id) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                   OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'' 
                   OR UPPER(a.srt_no) LIKE ''%'' + UPPER(''' + @keyword + ''') + ''%'')
            AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)';
            
            -- SIMPLE FIX: Use IN clause for multiple status
            IF @status IS NOT NULL AND @status != ''
            BEGIN
                -- Replace comma with quotes for IN clause
                DECLARE @statusIN VARCHAR(MAX) = '''' + REPLACE(@status, ',', ''',''') + '''';
                SET @countSQL = @countSQL + ' AND a.pdi_status IN (' + @statusIN + ')';
            END
            ELSE IF @pdi_status IS NOT NULL AND @pdi_status != ''
            BEGIN
                SET @countSQL = @countSQL + ' AND a.pdi_status = ''' + @pdi_status + '''';
            END
            
            EXEC sp_executesql @countSQL, N'@TotalRecords INT OUTPUT', @TotalRecords OUTPUT;
        END
        
        DECLARE @sql NVARCHAR(MAX) = '
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
        AND b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)';
        
        -- SIMPLE FIX: Use IN clause for multiple status
        IF @status IS NOT NULL AND @status != ''
        BEGIN
            -- Replace comma with quotes for IN clause
            DECLARE @statusIN2 VARCHAR(MAX) = '''' + REPLACE(@status, ',', ''',''') + '''';
            SET @sql = @sql + ' AND a.pdi_status IN (' + @statusIN2 + ')';
        END
        ELSE IF @pdi_status IS NOT NULL AND @pdi_status != ''
        BEGIN
            SET @sql = @sql + ' AND a.pdi_status = ''' + @pdi_status + '''';
        END
        
        SET @sql = @sql + ' ORDER BY ';
        
        -- Add proper ORDER BY clause
        IF @order_by = 'no asc'
            SET @sql = @sql + 'a.pdi_id ASC';
        ELSE IF @order_by = 'no desc'
            SET @sql = @sql + 'a.pdi_id DESC';
        ELSE IF @order_by = 'nim asc'
            SET @sql = @sql + 'a.mhs_id ASC';
        ELSE IF @order_by = 'nim desc'
            SET @sql = @sql + 'a.mhs_id DESC';
        ELSE IF @order_by = 'pdi_created_date asc'
            SET @sql = @sql + 'a.pdi_created_date ASC';
        ELSE
            SET @sql = @sql + 'a.pdi_created_date DESC'; -- default
        
        SET @sql = @sql + '
        OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
        FETCH NEXT ' + CAST(ISNULL(@PageSize, 2147483647) AS VARCHAR) + ' ROWS ONLY';
        
        EXEC(@sql);
    END
END
GO

PRINT '✅ SP sia_getDataRiwayatPengunduranDiri Mode 4 SIMPLE FIX applied!'
PRINT '   - Uses simple IN clause instead of CHARINDEX'
PRINT '   - Converts comma-separated status to IN (''status1'',''status2'',''status3'')'
PRINT '   - More reliable and easier to debug'
GO

-- Test the SIMPLE fixed SP with multiple status
PRINT 'Testing Mode 4 with SIMPLE FIX...'
DECLARE @total INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'nda_admin',
    @pdi_status = 'Disetujui,Ditolak Prodi,Ditolak Wadir1',  -- This will trigger Mode 4
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @status = 'Disetujui,Ditolak Prodi,Ditolak Wadir1',      -- This will be used for filtering
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;
    
PRINT 'SIMPLE FIX test completed. Total records: ' + CAST(@total AS VARCHAR);
GO