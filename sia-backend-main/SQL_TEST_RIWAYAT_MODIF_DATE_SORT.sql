-- Test sorting Riwayat by modif_date (recently updated on top)
USE [ERP_PolmanAstra_NDA]
GO

-- Test 1: Check raw data with modif_date
PRINT '=== TEST 1: Raw Data with Modif Date ==='
SELECT TOP 10
    dro_id,
    mhs_id,
    dro_status,
    dro_created_date,
    dro_modif_date,
    COALESCE(dro_modif_date, dro_created_date) AS effective_date,
    CONVERT(VARCHAR(11), COALESCE(dro_modif_date, dro_created_date), 106) AS formatted_date
FROM sia_msdropout
WHERE dro_status = 'Disetujui'
ORDER BY COALESCE(dro_modif_date, dro_created_date) DESC;

-- Test 2: Execute SP with empty sortBy (should use default = modif_date DESC)
PRINT '=== TEST 2: SP with Default Sorting (Empty sortBy) ==='
EXEC sia_getDataRiwayatDO
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = '',  -- Empty = use default (modif_date DESC)
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = 'Disetujui',
    @Page = 1,
    @PageSize = 10;

-- Test 3: Check specific record that was just updated (upload SK)
PRINT '=== TEST 3: Check Recently Updated Record ==='
-- Replace 'YOUR_DRO_ID' with the actual ID you just uploaded SK for
-- SELECT 
--     dro_id,
--     dro_status,
--     dro_created_date,
--     dro_modif_date,
--     dro_sk,
--     dro_skpb
-- FROM sia_msdropout
-- WHERE dro_id = 'YOUR_DRO_ID';
