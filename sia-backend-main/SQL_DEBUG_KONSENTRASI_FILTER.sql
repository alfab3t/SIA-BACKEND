-- =============================================
-- Debug: Konsentrasi Filter Issue
-- Problem: When sending konsentrasi ID, no results returned
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- 1. CEK DATA KONSENTRASI YANG ADA
PRINT '=== 1. CEK DATA KONSENTRASI ==='
SELECT TOP 10
    k.kon_id,
    k.kon_nama,
    k.kon_singkatan,
    p.pro_nama,
    COUNT(m.mhs_id) as total_mahasiswa
FROM sia_mskonsentrasi k
LEFT JOIN sia_msprodi p ON k.pro_id = p.pro_id
LEFT JOIN sia_msmahasiswa m ON k.kon_id = m.kon_id
GROUP BY k.kon_id, k.kon_nama, k.kon_singkatan, p.pro_nama
ORDER BY k.kon_id
GO

-- 2. CEK DATA DROPOUT BERDASARKAN KONSENTRASI
PRINT '=== 2. CEK DATA DROPOUT PER KONSENTRASI ==='
SELECT 
    c.kon_id,
    c.kon_nama,
    COUNT(a.dro_id) as total_dropout
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
GROUP BY c.kon_id, c.kon_nama
ORDER BY c.kon_id
GO

-- 3. TEST FILTER DENGAN ID KONSENTRASI SPESIFIK
PRINT '=== 3. TEST FILTER DENGAN kon_id = 11 ==='
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
WHERE c.kon_id = '11'  -- Test dengan ID konsentrasi
GO

-- 4. TEST FILTER DENGAN NAMA KONSENTRASI
PRINT '=== 4. TEST FILTER DENGAN kon_nama ==='
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
WHERE c.kon_nama = 'Teknologi Rekayasa Pemeliharaan Alat Berat'
GO

-- 5. CEK CURRENT STORED PROCEDURE LOGIC
PRINT '=== 5. CEK CURRENT SP LOGIC ==='
PRINT 'Current filter in SP:'
PRINT 'WHERE b.kon_id = (CASE WHEN @kon_id = '''' THEN b.kon_id ELSE @kon_id END)'
PRINT ''
PRINT 'This means:'
PRINT '- If @kon_id is empty: show all konsentrasi'
PRINT '- If @kon_id has value: filter by b.kon_id = @kon_id'
PRINT ''

-- 6. TEST MANUAL DENGAN LOGIC SP SAAT INI
PRINT '=== 6. TEST MANUAL DENGAN LOGIC SP ==='
DECLARE @kon_id VARCHAR(50) = '11'  -- Simulate parameter

SELECT 
    a.dro_id,
    b.mhs_id,
    b.mhs_nama,
    c.kon_nama,
    a.dro_status
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
WHERE b.kon_id = (CASE WHEN @kon_id = '' THEN b.kon_id ELSE @kon_id END)
GO

-- 7. CEK TIPE DATA KOLOM kon_id
PRINT '=== 7. CEK TIPE DATA KOLOM kon_id ==='
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE COLUMN_NAME = 'kon_id'
  AND TABLE_NAME IN ('sia_mskonsentrasi', 'sia_msmahasiswa')
ORDER BY TABLE_NAME
GO

-- 8. TEST DIRECT SP CALL
PRINT '=== 8. TEST DIRECT SP CALL ==='
PRINT 'Testing with kon_id = 11'

EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '11',  -- ID konsentrasi
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10
GO

PRINT '=== DEBUG COMPLETED ==='
PRINT 'Check results above to identify the issue'
GO