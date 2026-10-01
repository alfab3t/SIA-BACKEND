-- =============================================
-- Test Script: sia_getDataPengunduranDiri (Updated)
-- Testing all scenarios: Mahasiswa, Karyawan, Multiple Status
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

PRINT '========================================';
PRINT 'TEST SUITE: sia_getDataPengunduranDiri';
PRINT '========================================';
PRINT '';

-- =============================================
-- TEST 1: Mahasiswa melihat data sendiri
-- =============================================
PRINT '--- TEST 1: Mahasiswa Login (NIM) ---';
PRINT 'Expected: Hanya data mahasiswa dengan NIM tersebut';
PRINT '';

EXEC sia_getDataPengunduranDiri
    @username = '2101010001',  -- Ganti dengan NIM yang ada di database
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

PRINT '';
PRINT '--- END TEST 1 ---';
PRINT '';

-- =============================================
-- TEST 2: Wadir 1 melihat pending approval
-- =============================================
PRINT '--- TEST 2: Wadir 1 Login (str_main_id = 2) ---';
PRINT 'Expected: Data dengan status "Belum Disetujui Wadir 1"';
PRINT '';

-- Cari username Wadir 1
DECLARE @wadir1_username VARCHAR(50);
SELECT TOP 1 @wadir1_username = kry_username 
FROM ess_mskaryawan 
WHERE str_main_id = '2';

IF @wadir1_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @wadir1_username;
    
    EXEC sia_getDataPengunduranDiri
        @username = @wadir1_username,
        @pdi_status = '',
        @kry_id = '',
        @status = '',
        @Page = 1,
        @PageSize = 10;
END
ELSE
BEGIN
    PRINT 'WARNING: No Wadir 1 user found (str_main_id = 2)';
END

PRINT '';
PRINT '--- END TEST 2 ---';
PRINT '';

-- =============================================
-- TEST 3: Direktur melihat multiple status
-- =============================================
PRINT '--- TEST 3: Direktur Login (str_main_id = 1) ---';
PRINT 'Expected: Data dengan status Belum Disetujui Direktur, Wadir 1, Draft, Revisi';
PRINT '';

-- Cari username Direktur
DECLARE @direktur_username VARCHAR(50);
SELECT TOP 1 @direktur_username = kry_username 
FROM ess_mskaryawan 
WHERE str_main_id = '1';

IF @direktur_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @direktur_username;
    
    EXEC sia_getDataPengunduranDiri
        @username = @direktur_username,
        @pdi_status = '',
        @kry_id = '',
        @status = '',
        @Page = 1,
        @PageSize = 10;
END
ELSE
BEGIN
    PRINT 'WARNING: No Direktur user found (str_main_id = 1)';
END

PRINT '';
PRINT '--- END TEST 3 ---';
PRINT '';

-- =============================================
-- TEST 4: Admin melihat hampir semua status
-- =============================================
PRINT '--- TEST 4: Admin Login (str_main_id = 14) ---';
PRINT 'Expected: Data dengan status Draft, Belum Disetujui Wadir 1, Direktur, Revisi, Menunggu Upload SK';
PRINT '';

-- Cari username Admin
DECLARE @admin_username VARCHAR(50);
SELECT TOP 1 @admin_username = kry_username 
FROM ess_mskaryawan 
WHERE str_main_id IN ('14', '54', '26', '19');

IF @admin_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @admin_username;
    
    EXEC sia_getDataPengunduranDiri
        @username = @admin_username,
        @pdi_status = '',
        @kry_id = '',
        @status = '',
        @Page = 1,
        @PageSize = 10;
END
ELSE
BEGIN
    PRINT 'WARNING: No Admin user found (str_main_id = 14, 54, 26, 19)';
END

PRINT '';
PRINT '--- END TEST 4 ---';
PRINT '';

-- =============================================
-- TEST 5: Sekprodi melihat data konsentrasi
-- =============================================
PRINT '--- TEST 5: Sekprodi Login ---';
PRINT 'Expected: Data mahasiswa di konsentrasi yang dikelola Sekprodi';
PRINT '';

-- Cari Sekprodi yang ada di sia_mskonsentrasi
DECLARE @sekprodi_kryid VARCHAR(50);
DECLARE @sekprodi_username VARCHAR(50);

SELECT TOP 1 @sekprodi_kryid = kon_sekprodi 
FROM sia_mskonsentrasi 
WHERE kon_sekprodi IS NOT NULL AND kon_sekprodi != '';

IF @sekprodi_kryid IS NOT NULL
BEGIN
    SELECT @sekprodi_username = kry_username 
    FROM ess_mskaryawan 
    WHERE kry_id = @sekprodi_kryid;
    
    IF @sekprodi_username IS NOT NULL
    BEGIN
        PRINT 'Testing with Sekprodi username: ' + @sekprodi_username;
        
        EXEC sia_getDataPengunduranDiri
            @username = @sekprodi_username,
            @pdi_status = '',
            @kry_id = '',
            @status = '',
            @Page = 1,
            @PageSize = 10;
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Sekprodi kry_id found but no username in ess_mskaryawan';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: No Sekprodi found in sia_mskonsentrasi';
END

PRINT '';
PRINT '--- END TEST 5 ---';
PRINT '';

-- =============================================
-- TEST 6: Staff Upload SK
-- =============================================
PRINT '--- TEST 6: Staff Upload SK (str_main_id = 27, 23, 28) ---';
PRINT 'Expected: Data dengan status Disetujui, Menunggu Upload SK';
PRINT '';

-- Cari username Staff Upload SK
DECLARE @staff_username VARCHAR(50);
SELECT TOP 1 @staff_username = kry_username 
FROM ess_mskaryawan 
WHERE str_main_id IN ('27', '23', '28');

IF @staff_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @staff_username;
    
    EXEC sia_getDataPengunduranDiri
        @username = @staff_username,
        @pdi_status = '',
        @kry_id = '',
        @status = '',
        @Page = 1,
        @PageSize = 10;
END
ELSE
BEGIN
    PRINT 'WARNING: No Staff Upload SK user found (str_main_id = 27, 23, 28)';
END

PRINT '';
PRINT '--- END TEST 6 ---';
PRINT '';

-- =============================================
-- TEST 7: Multiple Status Filter
-- =============================================
PRINT '--- TEST 7: Multiple Status Filter ---';
PRINT 'Expected: Data dengan status Draft ATAU Revisi';
PRINT '';

-- Gunakan admin username untuk test
IF @admin_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @admin_username;
    PRINT 'Status filter: Draft,Revisi';
    
    EXEC sia_getDataPengunduranDiri
        @username = @admin_username,
        @pdi_status = '',
        @kry_id = '',
        @status = 'Draft,Revisi',  -- Multiple status
        @Page = 1,
        @PageSize = 10;
END
ELSE
BEGIN
    PRINT 'WARNING: No admin username available for testing';
END

PRINT '';
PRINT '--- END TEST 7 ---';
PRINT '';

-- =============================================
-- TEST 8: Pagination Test
-- =============================================
PRINT '--- TEST 8: Pagination Test ---';
PRINT 'Expected: Page 2 with 5 records per page';
PRINT '';

IF @admin_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @admin_username;
    PRINT 'Page: 2, PageSize: 5';
    
    EXEC sia_getDataPengunduranDiri
        @username = @admin_username,
        @pdi_status = '',
        @kry_id = '',
        @status = '',
        @Page = 2,
        @PageSize = 5;
END
ELSE
BEGIN
    PRINT 'WARNING: No admin username available for testing';
END

PRINT '';
PRINT '--- END TEST 8 ---';
PRINT '';

-- =============================================
-- TEST 9: Sorting Test (Draft/Revisi on top)
-- =============================================
PRINT '--- TEST 9: Sorting Test ---';
PRINT 'Expected: Draft & Revisi on top, then sorted by date DESC';
PRINT '';

IF @admin_username IS NOT NULL
BEGIN
    PRINT 'Testing with username: ' + @admin_username;
    PRINT 'Check: Draft/Revisi should appear first';
    
    EXEC sia_getDataPengunduranDiri
        @username = @admin_username,
        @pdi_status = '',
        @kry_id = '',
        @status = '',
        @Page = 1,
        @PageSize = 20;
END
ELSE
BEGIN
    PRINT 'WARNING: No admin username available for testing';
END

PRINT '';
PRINT '--- END TEST 9 ---';
PRINT '';

-- =============================================
-- TEST 10: Empty Result Test
-- =============================================
PRINT '--- TEST 10: Empty Result Test ---';
PRINT 'Expected: Empty result with Count = 0';
PRINT '';

EXEC sia_getDataPengunduranDiri
    @username = 'nonexistent_user_12345',
    @pdi_status = '',
    @kry_id = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;

PRINT '';
PRINT '--- END TEST 10 ---';
PRINT '';

-- =============================================
-- SUMMARY
-- =============================================
PRINT '========================================';
PRINT 'TEST SUITE COMPLETED';
PRINT '========================================';
PRINT '';
PRINT 'Review results above and verify:';
PRINT '1. Mahasiswa only sees own data';
PRINT '2. Wadir 1 sees "Belum Disetujui Wadir 1"';
PRINT '3. Direktur sees multiple status';
PRINT '4. Admin sees most status';
PRINT '5. Sekprodi sees konsentrasi data';
PRINT '6. Staff sees "Disetujui" and "Menunggu Upload SK"';
PRINT '7. Multiple status filter works';
PRINT '8. Pagination works correctly';
PRINT '9. Sorting: Draft/Revisi on top';
PRINT '10. Empty result returns Count = 0';
PRINT '';
PRINT 'If all tests pass, SP is ready for production!';
PRINT '';
