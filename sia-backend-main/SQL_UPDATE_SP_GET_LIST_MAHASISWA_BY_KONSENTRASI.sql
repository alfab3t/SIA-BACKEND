-- =============================================
-- Update SP sia_getListMahasiswaByKonsentrasi2
-- Menyamakan parameter dengan format standard (named parameter)
-- Menghapus kebutuhan @p1-@p50, menggunakan @KonsentrasiId langsung
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- Drop existing SP if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sia_getListMahasiswaByKonsentrasi2]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sia_getListMahasiswaByKonsentrasi2]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- CREATE PROCEDURE sia_getListMahasiswaByKonsentrasi2
-- Parameter: @KonsentrasiId VARCHAR(MAX)
-- Sama dengan versi sebelumnya, tapi sekarang C# code menggunakan named parameter
-- =============================================
CREATE PROCEDURE [dbo].[sia_getListMahasiswaByKonsentrasi2]
    @KonsentrasiId VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        mhs_id, 
        mhs_id + ' - ' + mhs_nama AS mhs_nama
    FROM sia_msmahasiswa
    WHERE mhs_status = 'Aktif' 
        AND (mhs_status_kuliah = 'Aktif' OR mhs_status_kuliah = 'Menunggu Yudisium') 
        AND kon_id = @KonsentrasiId
    ORDER BY mhs_id ASC;
END
GO

PRINT 'SP sia_getListMahasiswaByKonsentrasi2 updated successfully'
PRINT 'C# code now uses @KonsentrasiId parameter (no more @p1-@p50)'
GO
