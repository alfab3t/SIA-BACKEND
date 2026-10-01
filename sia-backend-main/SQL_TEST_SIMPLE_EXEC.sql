-- =============================================
-- Test Simple: Exec SP dan Cek Hasilnya
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- 1. CEK STATUS SEBELUM
-- =============================================
PRINT '========================================='
PRINT 'STATUS SEBELUM EXEC SP'
PRINT '========================================='

SELECT 
    dro_id,
    dro_status,
    dro_created_date
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

-- =============================================
-- 2. EXEC SP (Seperti di screenshot)
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'EXEC SP'
PRINT '========================================='

DECLARE @return_value int

EXEC @return_value = [dbo].[sia_getIdDOByDraft]
    @dro_id_draft = N'16/PMA/DO/I/2026'

SELECT 'Return Value' = @return_value

GO

-- =============================================
-- 3. CEK STATUS SESUDAH
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'STATUS SESUDAH EXEC SP'
PRINT '========================================='

SELECT 
    dro_id,
    dro_status,
    dro_created_date
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

-- =============================================
-- 4. ANALISIS
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'ANALISIS'
PRINT '========================================='
PRINT 'Jika status BERUBAH: SP bekerja dengan baik'
PRINT 'Jika status TIDAK BERUBAH: Ada masalah dengan SP'
PRINT ''
PRINT 'Kemungkinan penyebab status tidak berubah:'
PRINT '1. SP tidak melakukan UPDATE'
PRINT '2. WHERE clause tidak match'
PRINT '3. Ada trigger yang rollback'
PRINT '4. Ada constraint yang mencegah update'
PRINT '========================================='
