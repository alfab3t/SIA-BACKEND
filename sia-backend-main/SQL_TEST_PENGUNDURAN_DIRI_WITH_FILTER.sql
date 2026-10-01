-- =============================================
-- Test Script: sia_getDataPengunduranDiri
-- Test dengan filter konsentrasi dan keyword
-- =============================================

-- Test 1: Mahasiswa (NIM) - lihat data sendiri
EXEC sia_getDataPengunduranDiri 
    @username = '2101010001',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Test 2: Wadir 1 - lihat yang perlu approval
EXEC sia_getDataPengunduranDiri 
    @username = 'wadir1',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = 'Belum Disetujui Wadir 1',
    @Page = 1,
    @PageSize = 10;

-- Test 3: Admin - dengan keyword search
EXEC sia_getDataPengunduranDiri 
    @username = 'admin',
    @keyword = 'budi',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Test 4: Admin - dengan filter konsentrasi (pro_id)
EXEC sia_getDataPengunduranDiri 
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = 'PRO001',  -- Ganti dengan pro_id yang valid
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Test 5: Admin - dengan filter konsentrasi (pro_nama)
EXEC sia_getDataPengunduranDiri 
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = 'Teknik Informatika',  -- Ganti dengan pro_nama yang valid
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Test 6: Admin - dengan multiple status
EXEC sia_getDataPengunduranDiri 
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = 'Draft,Revisi,Belum Disetujui Wadir 1',
    @Page = 1,
    @PageSize = 10;

-- Test 7: Admin - kombinasi keyword + konsentrasi + status
EXEC sia_getDataPengunduranDiri 
    @username = 'admin',
    @keyword = 'budi',
    @sort_by = '',
    @kon_id = 'Teknik Informatika',
    @pdi_status = '',
    @kry_id = '',
    @status = 'Draft,Revisi',
    @Page = 1,
    @PageSize = 10;

-- Test 8: Sekprodi - lihat data konsentrasi sendiri
EXEC sia_getDataPengunduranDiri 
    @username = 'sekprodi_ti',  -- Ganti dengan username sekprodi yang valid
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

-- Test 9: Staff SK - lihat yang perlu upload SK
EXEC sia_getDataPengunduranDiri 
    @username = 'staff_sk',  -- Ganti dengan username staff SK yang valid
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = 'Disetujui,Menunggu Upload SK',
    @Page = 1,
    @PageSize = 10;

-- Test 10: Pagination - page 2
EXEC sia_getDataPengunduranDiri 
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 2,
    @PageSize = 10;
