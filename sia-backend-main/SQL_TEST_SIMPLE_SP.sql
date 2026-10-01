-- Test simple SP execution
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