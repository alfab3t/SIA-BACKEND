-- Check current SP definition
USE [ERP_PolmanAstra_NDA]
GO

SELECT 
    OBJECT_NAME(object_id) AS ProcedureName,
    definition
FROM sys.sql_modules 
WHERE object_id = OBJECT_ID('sia_getDataPengunduranDiri');

-- Also check if SP exists
SELECT name, create_date, modify_date
FROM sys.procedures 
WHERE name = 'sia_getDataPengunduranDiri';