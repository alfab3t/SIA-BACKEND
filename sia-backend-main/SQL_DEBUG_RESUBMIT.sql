-- =============================================
-- Debug Script: Cek kenapa backend tidak update status
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- =============================================
-- 1. CEK DATA SEBELUM EXEC SP
-- =============================================
PRINT '========================================='
PRINT '1. CEK DATA SEBELUM'
PRINT '========================================='

SELECT 
    dro_id AS 'ID',
    dro_status AS 'Status',
    dro_created_date AS 'Created Date',
    dro_created_by AS 'Created By',
    dro_updated_date AS 'Updated Date',
    dro_updated_by AS 'Updated By'
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

-- =============================================
-- 2. EXEC SP LANGSUNG
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '2. EXEC SP'
PRINT '========================================='

EXEC sia_getIdDOByDraft @dro_id_draft = '16/PMA/DO/I/2026'

-- =============================================
-- 3. CEK DATA SESUDAH EXEC SP
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '3. CEK DATA SESUDAH'
PRINT '========================================='

SELECT 
    dro_id AS 'ID',
    dro_status AS 'Status',
    dro_created_date AS 'Created Date',
    dro_created_by AS 'Created By',
    dro_updated_date AS 'Updated Date',
    dro_updated_by AS 'Updated By'
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

-- =============================================
-- 4. CEK APAKAH ADA TRIGGER YANG MENCEGAH UPDATE
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '4. CEK TRIGGER'
PRINT '========================================='

SELECT 
    t.name AS 'Trigger Name',
    t.is_disabled AS 'Is Disabled',
    OBJECT_NAME(t.parent_id) AS 'Table Name',
    te.type_desc AS 'Event Type'
FROM sys.triggers t
INNER JOIN sys.trigger_events te ON t.object_id = te.object_id
WHERE OBJECT_NAME(t.parent_id) = 'sia_msdropout'

-- =============================================
-- 5. CEK PERMISSION
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '5. CEK PERMISSION'
PRINT '========================================='

SELECT 
    USER_NAME() AS 'Current User',
    HAS_PERMS_BY_NAME('sia_msdropout', 'OBJECT', 'UPDATE') AS 'Has UPDATE Permission',
    HAS_PERMS_BY_NAME('sia_getIdDOByDraft', 'OBJECT', 'EXECUTE') AS 'Has EXECUTE Permission'

-- =============================================
-- 6. TEST UPDATE LANGSUNG (TANPA SP)
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '6. TEST UPDATE LANGSUNG'
PRINT '========================================='

-- Backup status lama
DECLARE @oldStatus VARCHAR(50)
SELECT @oldStatus = dro_status FROM sia_msdropout WHERE dro_id = '16/PMA/DO/I/2026'
PRINT 'Status lama: ' + @oldStatus

-- Update langsung
UPDATE sia_msdropout
SET dro_status = 'TEST UPDATE LANGSUNG',
    dro_updated_date = GETDATE()
WHERE dro_id = '16/PMA/DO/I/2026'

-- Cek hasil
SELECT dro_status AS 'Status After Direct Update' 
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

-- Rollback ke status lama
UPDATE sia_msdropout
SET dro_status = @oldStatus
WHERE dro_id = '16/PMA/DO/I/2026'

PRINT 'Status dikembalikan ke: ' + @oldStatus

-- =============================================
-- 7. CEK APAKAH SP BENAR-BENAR DIJALANKAN
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '7. CEK SP DEFINITION'
PRINT '========================================='

SELECT 
    OBJECT_DEFINITION(OBJECT_ID('sia_getIdDOByDraft')) AS 'SP Definition'

-- =============================================
-- 8. SIMULASI BACKEND CALL
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '8. SIMULASI BACKEND CALL'
PRINT '========================================='

-- Simulasi seperti backend memanggil SP
DECLARE @testId VARCHAR(50) = '16/PMA/DO/I/2026'

PRINT 'Input parameter: ' + @testId

-- Cek sebelum
SELECT 'BEFORE' AS 'Timing', dro_status, dro_created_date 
FROM sia_msdropout 
WHERE dro_id = @testId

-- Execute SP
DECLARE @result TABLE (dro_id VARCHAR(50))
INSERT INTO @result
EXEC sia_getIdDOByDraft @dro_id_draft = @testId

-- Tampilkan result
SELECT 'SP Result: ' + dro_id FROM @result

-- Cek sesudah
SELECT 'AFTER' AS 'Timing', dro_status, dro_created_date 
FROM sia_msdropout 
WHERE dro_id = @testId

-- =============================================
-- 9. CEK TRANSACTION ISOLATION LEVEL
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '9. CEK TRANSACTION SETTINGS'
PRINT '========================================='

SELECT 
    CASE transaction_isolation_level 
        WHEN 0 THEN 'Unspecified' 
        WHEN 1 THEN 'ReadUncommitted' 
        WHEN 2 THEN 'ReadCommitted' 
        WHEN 3 THEN 'Repeatable' 
        WHEN 4 THEN 'Serializable' 
        WHEN 5 THEN 'Snapshot' 
    END AS 'Isolation Level'
FROM sys.dm_exec_sessions 
WHERE session_id = @@SPID

-- =============================================
-- 10. CEK APAKAH ADA MULTIPLE DATABASE
-- =============================================
PRINT ''
PRINT '========================================='
PRINT '10. CEK DATABASE'
PRINT '========================================='

SELECT 
    DB_NAME() AS 'Current Database',
    @@SERVERNAME AS 'Server Name'

-- Cek apakah ada database lain dengan nama mirip
SELECT name AS 'Available Databases'
FROM sys.databases
WHERE name LIKE '%Polman%' OR name LIKE '%ERP%'

PRINT ''
PRINT '========================================='
PRINT 'DEBUG SELESAI'
PRINT '========================================='
