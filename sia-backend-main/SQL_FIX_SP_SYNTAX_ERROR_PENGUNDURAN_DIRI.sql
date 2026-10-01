-- =============================================
-- Fix SP: sia_getDataPengunduranDiri
-- Fix SQL syntax error "Incorrect syntax near the keyword 'AS'"
-- =============================================
USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sia_getDataPengunduranDiri]
@username VARCHAR(50),
@keyword VARCHAR(MAX) = '',
@sort_by VARCHAR(100) = '',
@kon_id VARCHAR(50) = '',
@pdi_status VARCHAR(50) = '',
@kry_id VARCHAR(50) = '',
@status VARCHAR(MAX) = '',
@Page INT = NULL,
@PageSize INT = NULL
AS
BEGIN
SET NOCOUNT ON;

-- Build base query
DECLARE @sql NVARCHAR(MAX) = '
SELECT 
    ROW_NUMBER() OVER (ORDER BY a.pdi_created_date DESC) AS rownum,
    a.pdi_id,
    b.mhs_id,
    b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
    d.pro_singkatan + '' ('' + c.kon_singkatan + '')'' AS kon_nama,
    CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS pdi_created_date,
    a.pdi_created_by,
    a.srt_no,
    a.pdi_status,
    COUNT(*) OVER() AS [Count]
FROM sia_mspengundurandiri a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
WHERE 1=1';

-- Add kon_id filter if provided
IF @kon_id IS NOT NULL AND @kon_id != ''
BEGIN
    SET @sql = @sql + ' AND (d.pro_id = ''' + REPLACE(@kon_id, '''', '''''') + ''' OR d.pro_nama LIKE ''%' + REPLACE(@kon_id, '''', '''''') + '%'')';
END

-- Add keyword filter if provided
IF @keyword IS NOT NULL AND @keyword != ''
BEGIN
    SET @sql = @sql + ' AND (a.pdi_id LIKE ''%' + REPLACE(@keyword, '''', '''''') + '%'' OR b.mhs_nama LIKE ''%' + REPLACE(@keyword, '''', '''''') + '%'' OR c.kon_nama LIKE ''%' + REPLACE(@keyword, '''', '''''') + '%'' OR a.srt_no LIKE ''%' + REPLACE(@keyword, '''', '''''') + '%'')';
END

-- Add ORDER BY
SET @sql = @sql + ' ORDER BY a.pdi_created_date DESC';

-- Add pagination if provided
IF @Page IS NOT NULL AND @PageSize IS NOT NULL
BEGIN
    DECLARE @offset INT = (@Page - 1) * @PageSize;
    SET @sql = 'SELECT * FROM (' + @sql + ') AS subquery WHERE rownum BETWEEN ' + 
               CAST(@offset + 1 AS VARCHAR) + ' AND ' + 
               CAST(@offset + @PageSize AS VARCHAR);
END

-- Execute the query
EXEC sp_executesql @sql;

END
GO