-- =============================================
-- FIX: sia_getIdDOByDraft
-- Pastikan UPDATE benar-benar dijalankan
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sia_getIdDOByDraft]
    @dro_id_draft VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @tempIdDO INT;
    DECLARE @tempRomawi VARCHAR(5);
    DECLARE @rowsAffected INT;
    
    -- Debug: Print input parameter
    PRINT 'Input ID: ' + @dro_id_draft;
    
    -- Update tanggal created dan status menjadi "Belum Disetujui Wadir 1"
    UPDATE sia_msdropout
    SET dro_created_date = GETDATE(),
        dro_status = 'Belum Disetujui Wadir 1'
    WHERE dro_id = @dro_id_draft;
    
    -- Cek berapa baris yang ter-update
    SET @rowsAffected = @@ROWCOUNT;
    PRINT 'Rows affected by UPDATE: ' + CAST(@rowsAffected AS VARCHAR);
    
    -- Jika tidak ada baris yang ter-update, return error
    IF @rowsAffected = 0
    BEGIN
        PRINT 'ERROR: No rows updated. ID not found: ' + @dro_id_draft;
        SELECT 'ERROR: Data tidak ditemukan' AS dro_id;
        RETURN;
    END
    
    -- Hitung nomor urut DO untuk bulan dan tahun ini
    SELECT @tempIdDO = (
        SELECT COUNT(*) + 1 
        FROM sia_msdropout 
        WHERE dro_id LIKE '%DO%' 
          AND MONTH(dro_created_date) = MONTH(GETDATE()) 
          AND YEAR(dro_created_date) = YEAR(GETDATE())
    );
    
    PRINT 'Nomor urut DO: ' + CAST(@tempIdDO AS VARCHAR);
    
    -- Convert bulan ke angka romawi
    SELECT @tempRomawi = dbo.fnConvertIntToRoman(MONTH(GETDATE()));
    PRINT 'Bulan romawi: ' + @tempRomawi;
    
    -- Jika masih draft (belum punya format DO), generate ID baru
    -- Format: {nomor}/PMA/DO/{bulan_romawi}/{tahun}
    -- Contoh: 1/PMA/DO/I/2026
    IF @dro_id_draft NOT LIKE '%DO%'
    BEGIN
        PRINT 'Generating new DO ID...';
        
        UPDATE sia_msdropout
        SET dro_id = CAST(@tempIdDO AS VARCHAR) + '/PMA/DO/' + @tempRomawi + '/' + CAST(YEAR(GETDATE()) AS VARCHAR)
        WHERE dro_id = @dro_id_draft;
        
        SET @rowsAffected = @@ROWCOUNT;
        PRINT 'Rows affected by ID update: ' + CAST(@rowsAffected AS VARCHAR);
    END
    ELSE
    BEGIN
        PRINT 'ID already in DO format, keeping same ID';
    END
    
    -- Return ID yang baru dibuat atau ID yang sama
    SELECT TOP 1 dro_id 
    FROM sia_msdropout 
    WHERE dro_id = @dro_id_draft 
       OR (@dro_id_draft NOT LIKE '%DO%' AND dro_id LIKE '%DO%' AND dro_created_date >= DATEADD(SECOND, -5, GETDATE()))
    ORDER BY dro_created_date DESC;
    
    PRINT 'SP execution completed';
END
GO

-- =============================================
-- TEST SCRIPT
-- =============================================

PRINT ''
PRINT '========================================='
PRINT 'TEST: Exec SP dengan Debug Messages'
PRINT '========================================='

-- Cek status sebelum
SELECT 'BEFORE' AS Timing, dro_id, dro_status, dro_created_date 
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

-- Exec SP
EXEC sia_getIdDOByDraft @dro_id_draft = '16/PMA/DO/I/2026'

-- Cek status sesudah
SELECT 'AFTER' AS Timing, dro_id, dro_status, dro_created_date 
FROM sia_msdropout 
WHERE dro_id = '16/PMA/DO/I/2026'

PRINT ''
PRINT '========================================='
PRINT 'Expected: Status berubah ke "Belum Disetujui Wadir 1"'
PRINT 'Expected: Created date berubah ke waktu sekarang'
PRINT '========================================='
