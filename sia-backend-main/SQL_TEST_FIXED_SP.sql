-- Test the fixed SP
-- First execute the fix, then test
USE [ERP_PolmanAstra_NDA]
GO

-- Test basic execution
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

-- Test with kon_id filter
EXEC sia_getDataPengunduranDiri 
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = 'TI',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;