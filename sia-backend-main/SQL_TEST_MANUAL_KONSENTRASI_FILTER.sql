-- =============================================
-- Manual Test: Konsentrasi Filter Logic
-- Test the exact filter logic used in the SP
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- Step 1: Check what konsentrasi data exists
PRINT '=== STEP 1: Available Konsentrasi Data ==='
SELECT 
    kon_id,
    kon_nama,
    kon_singkatan
FROM sia_mskonsentrasi
ORDER BY CAST(kon_id AS INT)
GO

-- Step 2: Check dropout data per konsentrasi
PRINT '=== STEP 2: Dropout Data per Konsentrasi ==='
SELECT 
    c.kon_id,
    c.kon_nama,
    COUNT(a.dro_id) as total_dropout,
    MIN(a.dro_created_date) as earliest_date,
    MAX(a.dro_created_date) as latest_date
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id, c.kon_nama
ORDER BY CAST(c.kon_id AS INT)
GO

-- Step 3: Test current SP filter logic manually
PRINT '=== STEP 3: Test Current SP Filter Logic ==='

-- Simulate the current SP filter: b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
DECLARE @test_kon_id VARCHAR(50) = '11'  -- Test with ID 11

PRINT 'Testing with @kon_id = ' + @test_kon_id

SELECT 
    'Current SP Logic' as test_type,
    COUNT(*) as total_records
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE b.kon_id = (CASE WHEN @test_kon_id = '' THEN b.kon_id ELSE @test_kon_id END)

-- Test with empty kon_id (should return all)
SET @test_kon_id = ''
PRINT 'Testing with @kon_id = empty (should return all)'

SELECT 
    'Empty kon_id' as test_type,
    COUNT(*) as total_records
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE b.kon_id = (CASE WHEN @test_kon_id = '' THEN b.kon_id ELSE @test_kon_id END)
GO

-- Step 4: Test improved filter logic
PRINT '=== STEP 4: Test Improved Filter Logic ==='

DECLARE @test_kon_id2 VARCHAR(50) = '11'

PRINT 'Testing improved logic with @kon_id = ' + @test_kon_id2

SELECT 
    'Improved Logic (ID)' as test_type,
    COUNT(*) as total_records
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE (@test_kon_id2 = '' OR b.kon_id = @test_kon_id2 OR c.kon_nama = @test_kon_id2)

-- Test with name
SET @test_kon_id2 = 'Teknologi Rekayasa Pemeliharaan Alat Berat'
PRINT 'Testing improved logic with @kon_id = ' + @test_kon_id2

SELECT 
    'Improved Logic (Name)' as test_type,
    COUNT(*) as total_records
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE (@test_kon_id2 = '' OR b.kon_id = @test_kon_id2 OR c.kon_nama = @test_kon_id2)
GO

-- Step 5: Check specific data for kon_id = 11
PRINT '=== STEP 5: Detailed Data for kon_id = 11 ==='
SELECT TOP 10
    a.dro_id,
    b.mhs_id,
    b.mhs_nama,
    c.kon_id,
    c.kon_nama,
    a.dro_status,
    a.dro_created_date
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE c.kon_id = '11'
ORDER BY a.dro_created_date DESC
GO

-- Step 6: Check data types
PRINT '=== STEP 6: Check Data Types ==='
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE COLUMN_NAME = 'kon_id'
  AND TABLE_NAME IN ('sia_mskonsentrasi', 'sia_msmahasiswa')
ORDER BY TABLE_NAME
GO

PRINT '=== MANUAL TEST COMPLETED ==='
PRINT 'Check the results above to identify the issue'
GO