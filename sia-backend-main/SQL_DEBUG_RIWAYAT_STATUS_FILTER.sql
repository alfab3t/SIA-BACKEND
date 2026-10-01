-- Debug: Check data dengan status filter
USE [ERP_PolmanAstra_NDA]
GO

-- Test 1: Tanpa status filter (semua data)
PRINT '=== TEST 1: Tanpa Status Filter ==='
EXEC sia_getDataRiwayatDO
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',  -- EMPTY = all status
    @Page = 1,
    @PageSize = 10;

-- Test 2: Dengan status filter 'Disetujui'
PRINT '=== TEST 2: Status Filter = Disetujui ==='
EXEC sia_getDataRiwayatDO
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = 'Disetujui',  -- Filter Disetujui only
    @Page = 1,
    @PageSize = 10;

-- Test 3: Check raw data dengan status
PRINT '=== TEST 3: Raw Data Top 10 ==='
SELECT TOP 10
    a.dro_id,
    a.mhs_id,
    a.dro_status,
    a.dro_created_date,
    CONVERT(VARCHAR(11), a.dro_created_date, 106) AS formatted_date
FROM sia_msdropout a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = c.pro_id
WHERE a.dro_status = 'Disetujui'  -- Same filter as backend
ORDER BY a.dro_created_date DESC;

-- Test 4: Check data 26 Mar 2026
PRINT '=== TEST 4: Check Data 26 Mar 2026 ==='
SELECT 
    dro_id,
    mhs_id,
    dro_status,
    dro_created_date,
    CONVERT(VARCHAR(11), dro_created_date, 106) AS formatted_date
FROM sia_msdropout
WHERE CONVERT(DATE, dro_created_date) = '2026-03-26'
ORDER BY dro_created_date DESC;
