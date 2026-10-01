-- =============================================
-- Cek Definisi SP yang Ada di Database
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

PRINT '========================================='
PRINT 'DEFINISI SP: sia_getIdDOByDraft'
PRINT '========================================='

-- Tampilkan definisi lengkap SP
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sia_getIdDOByDraft')) AS 'SP Definition'

PRINT ''
PRINT '========================================='
PRINT 'METADATA SP'
PRINT '========================================='

-- Tampilkan metadata SP
SELECT 
    OBJECT_NAME(object_id) AS 'SP Name',
    create_date AS 'Created Date',
    modify_date AS 'Last Modified Date',
    type_desc AS 'Type'
FROM sys.objects
WHERE object_id = OBJECT_ID('dbo.sia_getIdDOByDraft')

PRINT ''
PRINT '========================================='
PRINT 'PARAMETERS'
PRINT '========================================='

-- Tampilkan parameter SP
SELECT 
    name AS 'Parameter Name',
    TYPE_NAME(user_type_id) AS 'Data Type',
    max_length AS 'Max Length',
    is_output AS 'Is Output'
FROM sys.parameters
WHERE object_id = OBJECT_ID('dbo.sia_getIdDOByDraft')
ORDER BY parameter_id

PRINT ''
PRINT '========================================='
PRINT 'ANALISIS'
PRINT '========================================='
PRINT 'Cek apakah SP memiliki:'
PRINT '1. UPDATE statement untuk dro_status'
PRINT '2. UPDATE statement untuk dro_created_date'
PRINT '3. WHERE clause yang benar'
PRINT '========================================='
