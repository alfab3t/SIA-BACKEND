-- =============================================
-- Update SP sia_getListProdi
-- Menyamakan format nama prodi dengan sia_getDataRiwayatPengunduranDiri
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
    
    -- Format nama prodi: "pro_singkatan (kon_singkatan)"
    -- Sama dengan format di sia_getDataRiwayatPengunduranDiri
    SELECT 
        a.pro_id,
        a.pro_singkatan + ' (' + b.kon_singkatan + ')' AS pro_nama
    FROM sia_msprodi a
    INNER JOIN sia_mskonsentrasi b ON a.pro_id = b.pro_id
    WHERE a.pro_status = 'Aktif'
      AND b.kon_status = 'Aktif'
    ORDER BY a.pro_nama, b.kon_singkatan;
END
GO

-- =============================================
-- NOTES:
-- =============================================
-- Before: pro_jenjang + ' ' + pro_nama
-- Example: "D3 Teknik Informatika"
--
-- After: pro_singkatan + ' (' + kon_singkatan + ')'
-- Example: "TI (SE)", "TI (DS)", "TM (MFG)"
--
-- Keuntungan:
-- 1. Format konsisten dengan riwayat
-- 2. Lebih ringkas
-- 3. Menampilkan konsentrasi juga
-- 4. User bisa pilih prodi + konsentrasi sekaligus
-- =============================================
