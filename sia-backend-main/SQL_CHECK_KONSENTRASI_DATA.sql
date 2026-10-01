-- =============================================
-- Check: Konsentrasi Data Available
-- Purpose: Verify what konsentrasi data exists and has dropout records
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

PRINT '=== 1. ALL KONSENTRASI DATA ==='
SELECT 
    k.kon_id,
    k.kon_nama,
    k.kon_singkatan,
    p.pro_nama,
    k.kon_sekprodi
FROM sia_mskonsentrasi k
LEFT JOIN sia_msprodi p ON k.pro_id = p.pro_id
ORDER BY k.kon_id
GO

PRINT '=== 2. KONSENTRASI WITH MAHASISWA ==='
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

PRINT '=== 3. KONSENTRASI WITH DROPOUT DATA ==='
SELECT 
    c.kon_id,
    c.kon_nama,
    COUNT(a.dro_id) as total_dropout,
    STRING_AGG(a.dro_status, ', ') as status_list
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id, c.kon_nama
ORDER BY c.kon_id
GO

PRINT '=== 4. SAMPLE DROPOUT DATA PER KONSENTRASI ==='
SELECT TOP 20
    c.kon_id,
    c.kon_nama,
    a.dro_id,
    b.mhs_id,
    b.mhs_nama,
    a.dro_status,
    a.dro_created_date
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
ORDER BY c.kon_id, a.dro_created_date DESC
GO

PRINT '=== 5. CHECK DATA TYPES ==='
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE COLUMN_NAME = 'kon_id'
  AND TABLE_NAME IN ('sia_mskonsentrasi', 'sia_msmahasiswa')
ORDER BY TABLE_NAME
GO

PRINT '=== 6. TEST SPECIFIC KONSENTRASI ID ==='
-- Test dengan ID yang paling umum
DECLARE @test_id VARCHAR(10)
SELECT TOP 1 @test_id = c.kon_id
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id
ORDER BY COUNT(a.dro_id) DESC

PRINT 'Testing with most common kon_id: ' + @test_id

SELECT 
    'Direct Query Result' as test_type,
    COUNT(*) as total_records
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE c.kon_id = @test_id

PRINT 'If this shows 0 records, there might be a data issue'
GO

PRINT '=== DATA CHECK COMPLETED ==='
PRINT 'Use the results above to identify valid konsentrasi IDs for testing'
GO