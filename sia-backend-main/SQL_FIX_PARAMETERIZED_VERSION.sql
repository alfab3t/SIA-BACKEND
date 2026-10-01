-- =============================================
-- Alternative Fix: Use sp_executesql with Parameters
-- Avoid dynamic SQL concatenation issues completely
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
    DECLARE @params NVARCHAR(MAX);

    -- Ambil struktur organisasi dan karyawan ID
    SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
    SELECT @kryid = kry_id FROM ess_mskaryawan WHERE kry_username = @username;
    SELECT @kons = kon_id FROM sia_mskonsentrasi WHERE kon_sekprodi = @kryid;

    -- Logic filter berdasarkan role/struktur untuk PENGAJUAN
    IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    BEGIN
        SET @viewBy = ' AND a.mhs_id = @p_username';
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
        SET @viewBy = ' AND a.mhs_id = @p_username';
    END
    ELSE 
    BEGIN
        SET @viewBy = ' AND c.kon_sekprodi = @p_kryid';
    END;

    -- Build konsentrasi filter with parameters
    IF @kon_id IS NOT NULL AND @kon_id != ''
    BEGIN
        SET @konsentrasiFilter = ' AND (b.kon_id = @p_kon_id OR c.kon_nama = @p_kon_id)';
    END
    ELSE
    BEGIN
        SET @konsentrasiFilter = '';
    END

    -- Build status filter
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        SET @statusFilter = ' AND CHARINDEX('','' + a.dro_status + '','', @p_status_pattern) > 0';
    END

    -- Set default sorting
    IF @sort_by IS NULL OR @sort_by = ''
    BEGIN
        SET @sort_by = 'CASE WHEN a.dro_status IN (''Draft'', ''Revisi'') THEN 0 ELSE 1 END, COALESCE(a.dro_modif_date, a.dro_created_date) DESC';
    END

    -- Build parameterized SQL
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
        WHERE 1=1'
        + @konsentrasiFilter + '
          AND (a.dro_id LIKE ''%'' + @p_keyword + ''%'' 
               OR b.mhs_nama LIKE ''%'' + @p_keyword + ''%'' 
               OR c.kon_nama LIKE ''%'' + @p_keyword + ''%'' 
               OR a.srt_no LIKE ''%'' + @p_keyword + ''%'') '
          + @viewBy + @statusFilter + '
    ) res';

    -- Add pagination
    IF @Page IS NOT NULL AND @PageSize IS NOT NULL
    BEGIN
        SET @SQL = @SQL + ' WHERE rownum BETWEEN @p_start_row AND @p_end_row';
    END

    -- Define parameters
    SET @params = '@p_username VARCHAR(50), @p_keyword VARCHAR(MAX), @p_kon_id VARCHAR(50), @p_kryid VARCHAR(MAX), @p_status_pattern VARCHAR(MAX), @p_start_row INT, @p_end_row INT';

    -- Execute with parameters
    EXEC sp_executesql @SQL, @params, 
        @p_username = @username,
        @p_keyword = @keyword,
        @p_kon_id = @kon_id,
        @p_kryid = @kryid,
        @p_status_pattern = @statusPattern,
        @p_start_row = CASE WHEN @Page IS NOT NULL THEN (@Page - 1) * @PageSize + 1 ELSE NULL END,
        @p_end_row = CASE WHEN @Page IS NOT NULL THEN @Page * @PageSize ELSE NULL END;
END
GO

PRINT 'SP sia_getDataPendingDO updated with parameterized query approach'
GO

-- Test
PRINT '=== TEST: Parameterized Version ==='
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