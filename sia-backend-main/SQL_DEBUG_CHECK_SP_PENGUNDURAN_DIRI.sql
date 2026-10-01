-- =============================================
-- Debug: Check current SP definition
-- =============================================

-- Check if SP exists
SELECT 
    OBJECT_ID('sia_getDataPengunduranDiri') AS sp_object_id,
    CASE WHEN OBJECT_ID('sia_getDataPengunduranDiri') IS NOT NULL THEN 'EXISTS' ELSE 'NOT EXISTS' END AS sp_status;

-- Get SP definition
SELECT 
    OBJECT_DEFINITION(OBJECT_ID('sia_getDataPengunduranDiri')) AS sp_definition;

-- Get SP parameters
SELECT 
    p.name AS parameter_name,
    TYPE_NAME(p.user_type_id) AS parameter_type,
    p.max_length,
    p.is_output
FROM sys.parameters p
WHERE p.object_id = OBJECT_ID('sia_getDataPengunduranDiri')
ORDER BY p.parameter_id;

-- Test simple execution
EXEC sia_getDataPengunduranDiri 
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;
