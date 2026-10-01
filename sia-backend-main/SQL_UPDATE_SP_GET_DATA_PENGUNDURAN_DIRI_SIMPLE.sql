-- =============================================
-- Update SP: sia_getDataPengunduranDiri
-- SIMPLE VERSION - Basic functionality first
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

-- Simple static query first to test
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
ORDER BY a.pdi_created_date DESC;

END
GO