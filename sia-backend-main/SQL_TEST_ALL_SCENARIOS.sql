-- Test all scenarios after applying the fix
USE [ERP_PolmanAstra_NDA]
GO

PRINT '=== Testing Pengunduran Diri SP Fix ===';

-- Test 1: Basic call (should not error)
PRINT 'Test 1: Basic call';
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

-- Test 2: With kon_id filter (this was the main issue)
PRINT 'Test 2: With kon_id filter';
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

-- Test 3: With keyword search
PRINT 'Test 3: With keyword search';
EXEC sia_getDataPengunduranDiri 
    @username = 'nda_prodi',
    @keyword = 'test',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Test 4: With multiple filters
PRINT 'Test 4: With multiple filters';
EXEC sia_getDataPengunduranDiri 
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = 'Teknik',
    @pdi_status = '',
    @kry_id = '',
    @status = 'Draft,Belum Disetujui Prodi',
    @Page = 1,
    @PageSize = 5;

-- Test 5: Without pagination
PRINT 'Test 5: Without pagination';
EXEC sia_getDataPengunduranDiri 
    @username = 'nda_prodi',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = NULL,
    @PageSize = NULL;

PRINT '=== All tests completed ===';