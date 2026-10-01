-- =============================================
-- Test: Konsentrasi ID Filter Issue
-- Problem: No results when filtering by konsentrasi ID
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- Step 1: Cek data konsentrasi yang tersedia
PRINT '=== STEP 1: Available Konsentrasi Data ==='
SELECT 
    kon_id,
    kon_nama,
    kon_singkatan
FROM sia_mskonsentrasi
ORDER BY kon_id
GO

-- Step 2: Cek data mahasiswa per konsentrasi
PRINT '=== STEP 2: Mahasiswa per Konsentrasi ==='
SELECT 
    k.kon_id,
    k.kon_nama,
    COUNT(m.mhs_id) as total_mahasiswa
FROM sia_mskonsentrasi k
LEFT JOIN sia_msmahasiswa m ON k.kon_id = m.kon_id
GROUP BY k.kon_id, k.kon_nama
HAVING COUNT(m.mhs_id) > 0
ORDER BY k.kon_id
GO

-- Step 3: Cek data dropout per konsentrasi
PRINT '=== STEP 3: Dropout per Konsentrasi ==='
SELECT 
    c.kon_id,
    c.kon_nama,
    COUNT(a.dro_id) as total_dropout
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id, c.kon_nama
HAVING COUNT(a.dro_id) > 0
ORDER BY c.kon_id
GO

-- Step 4: Test filter dengan ID yang ada data dropout
PRINT '=== STEP 4: Test Filter dengan ID yang ada data ==='
-- Ambil kon_id pertama yang ada data dropout
DECLARE @test_kon_id VARCHAR(10)
SELECT TOP 1 @test_kon_id = c.kon_id
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
ORDER BY c.kon_id

PRINT 'Testing with kon_id: ' + @test_kon_id

SELECT 
    a.dro_id,
    b.mhs_id,
    b.mhs_nama,
    c.kon_id,
    c.kon_nama,
    a.dro_status
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE c.kon_id = @test_kon_id
GO

-- Step 5: Test stored procedure dengan ID yang valid
PRINT '=== STEP 5: Test SP dengan ID yang valid ==='
DECLARE @valid_kon_id VARCHAR(10)
SELECT TOP 1 @valid_kon_id = c.kon_id
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
ORDER BY c.kon_id

PRINT 'Testing SP with kon_id: ' + @valid_kon_id

EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = @valid_kon_id,
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10
GO

-- Step 6: Test tanpa filter konsentrasi (semua data)
PRINT '=== STEP 6: Test tanpa filter konsentrasi ==='
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',  -- Empty = all konsentrasi
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10
GO

-- Step 7: Cek apakah ada masalah dengan tipe data
PRINT '=== STEP 7: Check Data Types ==='
SELECT 
    'sia_mskonsentrasi' as table_name,
    'kon_id' as column_name,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'sia_mskonsentrasi' AND COLUMN_NAME = 'kon_id'

UNION ALL

SELECT 
    'sia_msmahasiswa' as table_name,
    'kon_id' as column_name,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'sia_msmahasiswa' AND COLUMN_NAME = 'kon_id'
GO

PRINT '=== TEST COMPLETED ==='
PRINT 'Check the results to identify why konsentrasi ID filter is not working'
GO