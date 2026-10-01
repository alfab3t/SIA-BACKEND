-- Check SP definition yang sekarang ada di database
USE [ERP_PolmanAstra_NDA]
GO

-- Lihat definisi SP yang sekarang aktif
SELECT OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO')) AS current_sp_definition;

-- Check apakah SP menggunakan ROW_NUMBER atau OFFSET/FETCH
SELECT 
    CASE 
        WHEN OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO')) LIKE '%ROW_NUMBER%' 
        THEN 'USING ROW_NUMBER (NEW VERSION)'
        WHEN OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO')) LIKE '%OFFSET%FETCH%' 
        THEN 'USING OFFSET/FETCH (OLD VERSION)'
        ELSE 'UNKNOWN VERSION'
    END AS sp_version,
    CASE 
        WHEN OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO')) LIKE '%COUNT(*) OVER()%' 
        THEN 'HAS Count COLUMN'
        ELSE 'NO Count COLUMN'
    END AS has_count_column,
    CASE 
        WHEN OBJECT_DEFINITION(OBJECT_ID('sia_getDataRiwayatDO')) LIKE '%@sort_by IS NULL OR @sort_by = ''''%' 
        THEN 'HAS DEFAULT SORTING LOGIC'
        ELSE 'NO DEFAULT SORTING'
    END AS has_default_sort;

-- Check last modified date
SELECT 
    name,
    create_date,
    modify_date,
    DATEDIFF(MINUTE, modify_date, GETDATE()) AS minutes_since_last_update
FROM sys.objects
WHERE name = 'sia_getDataRiwayatDO';
