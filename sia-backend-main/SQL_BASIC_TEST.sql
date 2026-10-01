-- =============================================
-- Basic Test: Just check if SP works
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- Test paling basic - tanpa filter apapun
PRINT '=== BASIC TEST: No filters ==='
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 2
GO

PRINT 'Basic test completed'
GO