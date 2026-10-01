-- Test script untuk verify sorting di sia_getDataRiwayatDO
-- Pastikan SP sudah di-update dengan ROW_NUMBER version

USE [ERP_PolmanAstra_NDA]
GO

-- Test 1: Tanpa sortBy (harus default ke newest first)
PRINT '=== TEST 1: Default Sorting (Empty sortBy) ==='
EXEC sia_getDataRiwayatDO
    @username = '',
    @keyword = '',
    @sort_by = '',  -- EMPTY = should use default
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 5;

-- Test 2: Explicit newest first
PRINT '=== TEST 2: Explicit DESC Sorting ==='
EXEC sia_getDataRiwayatDO
    @username = '',
    @keyword = '',
    @sort_by = 'a.dro_created_date DESC',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 5;

-- Test 3: Check raw data order
PRINT '=== TEST 3: Raw Data (Top 10 by date) ==='
SELECT TOP 10
    dro_id,
    mhs_id,
    dro_status,
    dro_created_date,
    CONVERT(VARCHAR(11), dro_created_date, 106) AS formatted_date
FROM sia_msdropout
ORDER BY dro_created_date DESC;

-- Test 4: Verify SP definition
PRINT '=== TEST 4: Check SP Definition ==='
SELECT 
    OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO')) AS sp_definition;
