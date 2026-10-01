-- =============================================
-- CREATE Stored Procedures untuk Pagination Drop Out
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- SP: sia_getRiwayatDropOutPaginated
-- Get Riwayat Drop Out dengan Pagination
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sia_getRiwayatDropOutPaginated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sia_getRiwayatDropOutPaginated]
GO

CREATE PROCEDURE [dbo].[sia_getRiwayatDropOutPaginated]
    @Username VARCHAR(MAX),
    @Keyword VARCHAR(MAX) = '',
    @SortBy VARCHAR(MAX) = 'a.dro_created_date desc',
    @Konsentrasi VARCHAR(MAX) = '',
    @Role VARCHAR(MAX) = '',
    @DisplayName VARCHAR(MAX) = '',
    @Page INT = 1,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Get total count
    SELECT @TotalRecords = COUNT(*)
    FROM tbl_dropout a
    LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    LEFT JOIN sia_msprodi d ON c.prd_id = d.prd_id
    LEFT JOIN sia_mssurat e ON a.srt_id = e.srt_id
    WHERE 
        (@Keyword = '' OR b.mhs_nama LIKE '%' + @Keyword + '%' OR a.mhs_id LIKE '%' + @Keyword + '%')
        AND (@Konsentrasi = '' OR c.kon_id = @Konsentrasi)
        AND (
            @Role = 'admin' 
            OR @Role = 'wadir1'
            OR (@Role = 'sekprodi' AND e.str_main_id = @DisplayName)
        );

    -- Get paginated data
    DECLARE @SQL NVARCHAR(MAX) = '
    SELECT 
        a.dro_id,
        a.mhs_id,
        b.mhs_nama,
        d.prd_nama,
        c.kon_nama,
        a.dro_status,
        a.dro_created_date,
        a.dro_modified_date,
        e.srt_no,
        a.dro_alasan,
        a.dro_catatan_wadir1
    FROM tbl_dropout a
    LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    LEFT JOIN sia_msprodi d ON c.prd_id = d.prd_id
    LEFT JOIN sia_mssurat e ON a.srt_id = e.srt_id
    WHERE 
        (@Keyword = '''' OR b.mhs_nama LIKE ''%'' + @Keyword + ''%'' OR a.mhs_id LIKE ''%'' + @Keyword + ''%'')
        AND (@Konsentrasi = '''' OR c.kon_id = @Konsentrasi)
        AND (
            @Role = ''admin'' 
            OR @Role = ''wadir1''
            OR (@Role = ''sekprodi'' AND e.str_main_id = @DisplayName)
        )
    ORDER BY ' + @SortBy + '
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @SQL,
        N'@Keyword VARCHAR(MAX), @Konsentrasi VARCHAR(MAX), @Role VARCHAR(MAX), @DisplayName VARCHAR(MAX), @Offset INT, @PageSize INT',
        @Keyword, @Konsentrasi, @Role, @DisplayName, @Offset, @PageSize;
END
GO

-- =============================================
-- SP: sia_getPendingDropOutPaginated
-- Get Pending Drop Out dengan Pagination
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sia_getPendingDropOutPaginated]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sia_getPendingDropOutPaginated]
GO

CREATE PROCEDURE [dbo].[sia_getPendingDropOutPaginated]
    @Username VARCHAR(MAX),
    @Keyword VARCHAR(MAX) = '',
    @SortBy VARCHAR(MAX) = 'a.dro_created_date desc',
    @Konsentrasi VARCHAR(MAX) = '',
    @Role VARCHAR(MAX) = '',
    @DisplayName VARCHAR(MAX) = '',
    @Page INT = 1,
    @PageSize INT = 10,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Get total count
    SELECT @TotalRecords = COUNT(*)
    FROM tbl_dropout a
    LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    LEFT JOIN sia_msprodi d ON c.prd_id = d.prd_id
    LEFT JOIN sia_mssurat e ON a.srt_id = e.srt_id
    WHERE 
        a.dro_status IN ('Draft', 'Menunggu Persetujuan Wadir 1')
        AND (@Keyword = '' OR b.mhs_nama LIKE '%' + @Keyword + '%' OR a.mhs_id LIKE '%' + @Keyword + '%')
        AND (@Konsentrasi = '' OR c.kon_id = @Konsentrasi)
        AND (
            @Role = 'admin' 
            OR @Role = 'wadir1'
            OR (@Role = 'sekprodi' AND e.str_main_id = @DisplayName)
        );

    -- Get paginated data
    DECLARE @SQL NVARCHAR(MAX) = '
    SELECT 
        a.dro_id,
        a.mhs_id,
        b.mhs_nama,
        d.prd_nama,
        c.kon_nama,
        a.dro_status,
        a.dro_created_date,
        a.dro_modified_date,
        e.srt_no,
        a.dro_alasan
    FROM tbl_dropout a
    LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    LEFT JOIN sia_msprodi d ON c.prd_id = d.prd_id
    LEFT JOIN sia_mssurat e ON a.srt_id = e.srt_id
    WHERE 
        a.dro_status IN (''Draft'', ''Menunggu Persetujuan Wadir 1'')
        AND (@Keyword = '''' OR b.mhs_nama LIKE ''%'' + @Keyword + ''%'' OR a.mhs_id LIKE ''%'' + @Keyword + ''%'')
        AND (@Konsentrasi = '''' OR c.kon_id = @Konsentrasi)
        AND (
            @Role = ''admin'' 
            OR @Role = ''wadir1''
            OR (@Role = ''sekprodi'' AND e.str_main_id = @DisplayName)
        )
    ORDER BY ' + @SortBy + '
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY';

    EXEC sp_executesql @SQL,
        N'@Keyword VARCHAR(MAX), @Konsentrasi VARCHAR(MAX), @Role VARCHAR(MAX), @DisplayName VARCHAR(MAX), @Offset INT, @PageSize INT',
        @Keyword, @Konsentrasi, @Role, @DisplayName, @Offset, @PageSize;
END
GO

PRINT 'Stored Procedures untuk Pagination Drop Out berhasil dibuat'
GO
