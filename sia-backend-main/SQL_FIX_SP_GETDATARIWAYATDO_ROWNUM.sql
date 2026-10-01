USE [ERP_PolmanAstra_NDA]
GO

/****** 
 * Fix: sia_getDataRiwayatDO - Use ROW_NUMBER for pagination
 * Add Count column for total records
 ******/

ALTER PROCEDURE [dbo].[sia_getDataRiwayatDO]
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
    DECLARE @str VARCHAR(MAX);

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
            SET @viewBy = ' AND a.dro_status IN (''Menunggu Upload SK'', ''Disetujui'', ''Draft'', ''Belum Disetujui Wadir 1'', ''Revisi'')';
        END 
        ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
        BEGIN
            SET @viewBy = ' AND a.dro_status IN (''Menunggu Upload SK'', ''Disetujui'', ''Draft'', ''Belum Disetujui Wadir 1'', ''Revisi'')';
        END 
        ELSE IF @str = '27' OR @str = '23' OR @str = '28' 
        BEGIN
            SET @viewBy = ' AND a.dro_status = ''Disetujui''';
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

    -- Build status filter
    IF @status IS NOT NULL AND @status != ''
    BEGIN
        DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
        SET @statusFilter = ' AND CHARINDEX('','' + a.dro_status + '','', ''' + @statusPattern + ''') > 0';
    END

    -- Set default sorting jika kosong
    -- Use dro_modif_date so recently updated records (e.g. SK upload) appear on top
    IF @sort_by IS NULL OR @sort_by = ''
    BEGIN
        SET @sort_by = 'COALESCE(a.dro_modif_date, a.dro_created_date) DESC';
    END

    -- Build dynamic SQL dengan ROW_NUMBER pagination
    SET @SQL = '
    SELECT * FROM (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY ' + @sort_by + ') AS rownum,
            a.dro_id,
            b.mhs_id,
            b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
            d.pro_nama AS kon_nama,
            CONVERT(VARCHAR(11), a.dro_created_date, 106) AS dro_created_date,
            a.dro_created_by,
            a.srt_no,
            a.dro_status,
            COUNT(*) OVER() AS [Count]
        FROM sia_msdropout a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
        INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
        INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
        WHERE b.kon_id = (CASE WHEN ''' + @kon_id + ''' = '''' THEN b.kon_id ELSE ''' + @kon_id + ''' END)
          AND (a.dro_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
               OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
               OR c.kon_singkatan LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
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

    EXEC(@SQL);
END
GO
