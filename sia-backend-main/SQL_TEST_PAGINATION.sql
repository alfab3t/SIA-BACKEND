-- =============================================
-- TEST Pagination - Cara yang Benar
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

PRINT '=== TEST 1: Dengan Pagination ==='
GO

-- Declare variable untuk total
DECLARE @total INT;

-- Execute SP dengan pagination
EXEC sia_getDataRiwayatDO 
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '3',
    @role_id = '',
    @display_name = '',
    @Page = 10,
    @PageSize = 10,
    @TotalRecords = @total OUTPUT;

-- Tampilkan total records
PRINT 'Total Records: ' + CAST(ISNULL(@total, 0) AS VARCHAR);
GO

PRINT ''
PRINT '=== TEST 2: Tanpa Pagination (Backward Compatible) ==='
GO

-- Test tanpa pagination
DECLARE @total2 INT;

EXEC sia_getDataRiwayatDO 
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '3',
    @role_id = '',
    @display_name = '',
    @Page = NULL,
    @PageSize = NULL,
    @TotalRecords = @total2 OUTPUT;

PRINT 'Total Records (should be NULL for backward compatibility): ' + CAST(ISNULL(@total2, 0) AS VARCHAR);
GO

PRINT ''
PRINT '=== TEST 3: Test Pengajuan (sia_getDataPendingDO) ==='
GO

DECLARE @total3 INT;

EXEC sia_getDataPendingDO 
    @username = 'nda_admin',
    @keyword = '',
    @sort_by = 'a.dro_created_date desc',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @Page = 1,
    @PageSize = 10,
    @TotalRecords = @total3 OUTPUT;

PRINT 'Total Records (Pengajuan): ' + CAST(ISNULL(@total3, 0) AS VARCHAR);
GO

PRINT ''
PRINT '=== TESTING COMPLETE ==='
PRINT 'Jika Anda melihat data di Results tab, berarti SP sudah bekerja dengan benar!'
PRINT 'Total Records menunjukkan jumlah total data (untuk pagination info)'
GO
