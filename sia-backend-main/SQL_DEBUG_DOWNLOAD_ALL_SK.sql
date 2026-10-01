-- =============================================
-- Debug: Download All SK Issue - Check SK and SKPB data
-- Issue: Only SKPB appears in download-all-sk endpoint, SK missing
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- 1. Check the specific record that has the issue
DECLARE @droId VARCHAR(50) = '12/PMA/DO/I/2026'  -- Replace with actual ID

PRINT '=== DEBUG: Download All SK Issue ==='
PRINT 'Checking data for ID: ' + @droId

-- 2. Check raw data from database
SELECT 
    dro_id,
    dro_sk as 'SK_Path_Raw',
    dro_skpb as 'SKPB_Path_Raw',
    LEN(dro_sk) as 'SK_Length',
    LEN(dro_skpb) as 'SKPB_Length',
    CASE WHEN dro_sk IS NULL THEN 'NULL'
         WHEN dro_sk = '' THEN 'EMPTY'
         ELSE 'HAS_VALUE' END as 'SK_Status',
    CASE WHEN dro_skpb IS NULL THEN 'NULL'
         WHEN dro_skpb = '' THEN 'EMPTY'
         ELSE 'HAS_VALUE' END as 'SKPB_Status'
FROM sia_msdropout 
WHERE dro_id = @droId

-- 3. Check what DownloadSKAsync stored procedure returns
PRINT '=== Testing sia_downloadSKDO SP ==='
EXEC sia_downloadSKDO @dro_id = @droId

-- 4. Check if files exist in expected locations
PRINT '=== File Path Analysis ==='
SELECT 
    dro_id,
    dro_sk,
    dro_skpb,
    -- Analyze SK path
    CASE 
        WHEN dro_sk LIKE '/%' OR dro_sk LIKE '\%' THEN 'Full path from root'
        WHEN dro_sk LIKE '%/%' THEN 'Relative path with folder'
        WHEN dro_sk NOT LIKE '%/%' AND dro_sk != '' THEN 'Filename only'
        ELSE 'Empty or NULL'
    END as 'SK_Path_Type',
    -- Analyze SKPB path
    CASE 
        WHEN dro_skpb LIKE '/%' OR dro_skpb LIKE '\%' THEN 'Full path from root'
        WHEN dro_skpb LIKE '%/%' THEN 'Relative path with folder'
        WHEN dro_skpb NOT LIKE '%/%' AND dro_skpb != '' THEN 'Filename only'
        ELSE 'Empty or NULL'
    END as 'SKPB_Path_Type'
FROM sia_msdropout 
WHERE dro_id = @droId

-- 5. Check all records that have both SK and SKPB
PRINT '=== Records with both SK and SKPB ==='
SELECT TOP 5
    dro_id,
    dro_sk,
    dro_skpb,
    dro_status
FROM sia_msdropout 
WHERE dro_sk IS NOT NULL 
  AND dro_sk != ''
  AND dro_skpb IS NOT NULL 
  AND dro_skpb != ''
ORDER BY dro_created_date DESC

-- 6. Check records with only SK
PRINT '=== Records with only SK (no SKPB) ==='
SELECT TOP 5
    dro_id,
    dro_sk,
    dro_skpb,
    dro_status
FROM sia_msdropout 
WHERE dro_sk IS NOT NULL 
  AND dro_sk != ''
  AND (dro_skpb IS NULL OR dro_skpb = '')
ORDER BY dro_created_date DESC

-- 7. Check records with only SKPB
PRINT '=== Records with only SKPB (no SK) ==='
SELECT TOP 5
    dro_id,
    dro_sk,
    dro_skpb,
    dro_status
FROM sia_msdropout 
WHERE (dro_sk IS NULL OR dro_sk = '')
  AND dro_skpb IS NOT NULL 
  AND dro_skpb != ''
ORDER BY dro_created_date DESC

PRINT 'Debug completed'
GO