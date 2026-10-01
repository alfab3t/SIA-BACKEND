-- =============================================
-- Update SP sia_getListProdi
-- Menghapus jenjang (D3/D4) dari output
-- Format: "Teknik Informatika" (tanpa jenjang)
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sia_getListProdi]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pro_id, 
        pro_nama AS pro_nama  -- Tanpa jenjang (D3/D4)
    FROM sia_msprodi
    WHERE pro_status = 'Aktif'
    ORDER BY pro_nama;
END
GO

-- =============================================
-- PERUBAHAN:
-- =============================================
-- BEFORE:
-- SELECT pro_id, pro_jenjang + ' ' + pro_nama AS pro_nama
-- Output: "D3 Teknik Informatika", "D4 Teknologi Rekayasa Pemeliharaan Alat Berat"
--
-- AFTER:
-- SELECT pro_id, pro_nama AS pro_nama
-- Output: "Teknik Informatika", "Teknologi Rekayasa Pemeliharaan Alat Berat"
--
-- =============================================
-- IMPACT:
-- =============================================
-- 1. Jenjang (D3/D4) dihapus dari output
-- 2. Hanya menampilkan nama prodi saja
-- 3. Frontend perlu update jika ada yang display jenjang
-- =============================================

PRINT '✓ SP sia_getListProdi updated successfully'
PRINT '  Format: pro_nama (tanpa jenjang D3/D4)'
GO
