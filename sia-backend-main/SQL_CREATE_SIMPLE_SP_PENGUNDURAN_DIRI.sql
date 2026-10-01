-- =============================================
-- Create Simple SP: sia_getDataPengunduranDiri
-- Simple version without dynamic SQL to avoid syntax errors
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

-- Simple query without dynamic SQL
WITH PaginatedData AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY a.pdi_created_date DESC) AS rownum,
        a.pdi_id,
        b.mhs_id,
        b.mhs_id + ' - ' + b.mhs_nama AS mhs_nama,
        d.pro_singkatan + ' (' + c.kon_singkatan + ')' AS kon_nama,
        CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS pdi_created_date,
        a.pdi_created_by,
        a.srt_no,
        a.pdi_status,
        COUNT(*) OVER() AS [Count]
    FROM sia_mspengundurandiri a
    INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
    WHERE 1=1
        AND (@kon_id = '' OR d.pro_id = @kon_id OR d.pro_nama LIKE '%' + @kon_id + '%')
        AND (@keyword = '' OR 
             a.pdi_id LIKE '%' + @keyword + '%' OR 
             b.mhs_nama LIKE '%' + @keyword + '%' OR 
             c.kon_nama LIKE '%' + @keyword + '%' OR 
             a.srt_no LIKE '%' + @keyword + '%')
)
SELECT *
FROM PaginatedData
WHERE (@Page IS NULL OR @PageSize IS NULL OR 
       (rownum BETWEEN ((@Page - 1) * @PageSize + 1) AND (@Page * @PageSize)))
ORDER BY rownum;

END
GO