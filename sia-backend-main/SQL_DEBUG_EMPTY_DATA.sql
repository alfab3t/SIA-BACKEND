-- =============================================
-- Debug Empty Data Issue in Riwayat API
-- Check what data exists and why the filter is not working
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

PRINT '=== DEBUGGING EMPTY DATA ISSUE ==='
PRINT ''

-- 1. Check if there's any data in the table
PRINT '1. Total records in sia_mspengundurandiri:'
SELECT COUNT(*) as total_records FROM sia_mspengundurandiri;
PRINT ''

-- 2. Check what status values exist
PRINT '2. Existing status values:'
SELECT DISTINCT pdi_status, COUNT(*) as count
FROM sia_mspengundurandiri 
GROUP BY pdi_status
ORDER BY pdi_status;
PRINT ''

-- 3. Check recent records
PRINT '3. Recent 5 records:'
SELECT TOP 5 
    pdi_id, 
    mhs_id, 
    pdi_status, 
    pdi_created_date,
    pdi_modif_date
FROM sia_mspengundurandiri 
ORDER BY pdi_created_date DESC;
PRINT ''

-- 4. Test the status filter logic manually
PRINT '4. Testing status filter logic:'
DECLARE @status VARCHAR(MAX) = 'Disetujui,Ditolak Prodi,Ditolak Wadir1';
DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';

PRINT 'Status parameter: ' + @status;
PRINT 'Status pattern: ' + @statusPattern;

-- Test records that should match
SELECT 
    pdi_id,
    mhs_id,
    pdi_status,
    CASE 
        WHEN CHARINDEX(',' + pdi_status + ',', @statusPattern) > 0 THEN 'MATCH'
        ELSE 'NO MATCH'
    END as filter_result
FROM sia_mspengundurandiri 
WHERE pdi_status IN ('Disetujui', 'Ditolak Prodi', 'Ditolak Wadir1')
ORDER BY pdi_created_date DESC;
PRINT ''

-- 5. Test the SP directly with simpler parameters
PRINT '5. Testing SP with single status:'
DECLARE @total1 INT;
EXEC sia_getDataRiwayatPengunduranDiri 
    @username = 'nda_admin',
    @pdi_status = 'Disetujui',  -- Single status
    @unused = '',
    @keyword = '',
    @order_by = 'pdi_created_date desc',
    @kon_id = '',
    @status = 'Disetujui',      -- Single status
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total1 OUTPUT;
    
PRINT 'Single status test - Total records: ' + CAST(@total1 AS VARCHAR);
PRINT ''

-- 6. Check if there are any records with the exact statuses we're looking for
PRINT '6. Records with target statuses:'
SELECT COUNT(*) as count, pdi_status
FROM sia_mspengundurandiri 
WHERE pdi_status IN ('Disetujui', 'Ditolak Prodi', 'Ditolak Wadir1')
GROUP BY pdi_status;
PRINT ''

-- 7. Check the SP definition to see what mode it's using
PRINT '7. Check what happens in Mode 4 (default mode):'
SELECT 
    pdi_id,
    mhs_id, 
    pdi_status,
    pdi_created_date
FROM sia_mspengundurandiri a
LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER('') + '%' 
       OR UPPER(b.mhs_nama) LIKE '%' + UPPER('') + '%' 
       OR UPPER(a.srt_no) LIKE '%' + UPPER('') + '%')
AND b.kon_id = (CASE WHEN '' = '' THEN b.kon_id ELSE '' END)
-- This is where the status filter should be applied
ORDER BY a.pdi_created_date DESC;

PRINT ''
PRINT '=== DEBUG COMPLETE ==='