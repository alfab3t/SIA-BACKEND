-- =============================================
-- Simple Test: Konsentrasi Filter
-- Test basic functionality first
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- Test 1: Tanpa filter konsentrasi (baseline)
PRINT '=== TEST 1: No Filter (Should work) ==='
EXEC sia_getDataPendingDO
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',  -- Empty
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 3
GO

-- Test 2: Cek data konsentrasi yang tersedia
PRINT '=== TEST 2: Available Konsentrasi Data ==='
SELECT TOP 5
    c.kon_id,
    c.kon_nama,
    COUNT(a.dro_id) as total_dropout
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id, c.kon_nama
ORDER BY COUNT(a.dro_id) DESC
GO

-- Test 3: Test dengan ID konsentrasi yang paling banyak data
PRINT '=== TEST 3: With Most Common Konsentrasi ID ==='
DECLARE @most_common_id VARCHAR(10)
SELECT TOP 1 @most_common_id = c.kon_id
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id
ORDER BY COUNT(a.dro_id) DESC

PRINT 'Testing with kon_id: ' + @most_common_id

EXEC sia_getDataPendingDO
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = @most_common_id,
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 3
GO

-- Test 4: Test dengan nama konsentrasi
PRINT '=== TEST 4: With Konsentrasi Name ==='
DECLARE @most_common_name VARCHAR(100)
SELECT TOP 1 @most_common_name = c.kon_nama
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id, c.kon_nama
ORDER BY COUNT(a.dro_id) DESC

PRINT 'Testing with kon_nama: ' + @most_common_name

EXEC sia_getDataPendingDO
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = @most_common_name,
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 3
GO

PRINT '=== ALL TESTS COMPLETED ==='
PRINT 'If any test fails, check the error message above'
GO