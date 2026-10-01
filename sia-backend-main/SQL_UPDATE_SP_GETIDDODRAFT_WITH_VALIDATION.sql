-- =============================================
-- Update SP: sia_getIdDOByDraft
-- Menambahkan validasi status sebelum mengajukan ulang
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
    DECLARE @currentStatus VARCHAR(50);
    DECLARE @errorMessage VARCHAR(500);
    
    -- Cek status saat ini jika ID sudah dalam format DO (bukan draft)
    IF @dro_id_draft LIKE '%DO%'
    BEGIN
        SELECT @currentStatus = dro_status 
        FROM sia_msdropout 
        WHERE dro_id = @dro_id_draft;
        
        -- Jika data tidak ditemukan
        IF @currentStatus IS NULL
        BEGIN
            -- Return error message
            SELECT 'ERROR: Data tidak ditemukan' AS dro_id;
            RETURN;
        END
        
        -- Validasi: Hanya status tertentu yang boleh diajukan ulang
        -- Status yang BOLEH diajukan ulang: Draft, Ditolak
        -- Status yang TIDAK BOLEH: Belum Disetujui Wadir 1, Disetujui Wadir 1, Selesai
        IF @currentStatus NOT IN ('Draft', 'Ditolak')
        BEGIN
            -- Return error dengan status saat ini
            SET @errorMessage = 'ERROR: Drop Out dengan status ''' + @currentStatus + ''' tidak dapat diajukan ulang';
            SELECT @errorMessage AS dro_id;
            RETURN;
        END
        
        PRINT 'Validasi OK: Status saat ini = ' + @currentStatus + ', boleh diajukan ulang';
    END
    
    -- Update tanggal created dan status menjadi "Belum Disetujui Wadir 1"
    UPDATE sia_msdropout
    SET dro_created_date = GETDATE(),
        dro_status = 'Belum Disetujui Wadir 1'
    WHERE dro_id = @dro_id_draft;
    
    -- Hitung nomor urut DO untuk bulan dan tahun ini
    SELECT @tempIdDO = (
        SELECT COUNT(*) + 1 
        FROM sia_msdropout 
        WHERE dro_id LIKE '%DO%' 
          AND MONTH(dro_created_date) = MONTH(GETDATE()) 
          AND YEAR(dro_created_date) = YEAR(GETDATE())
    );
    
    -- Convert bulan ke angka romawi
    SELECT @tempRomawi = dbo.fnConvertIntToRoman(MONTH(GETDATE()));
    
    -- Jika masih draft (belum punya format DO), generate ID baru
    -- Format: {nomor}/PMA/DO/{bulan_romawi}/{tahun}
    -- Contoh: 1/PMA/DO/I/2026
    IF @dro_id_draft NOT LIKE '%DO%'
    BEGIN
        UPDATE sia_msdropout
        SET dro_id = CAST(@tempIdDO AS VARCHAR) + '/PMA/DO/' + @tempRomawi + '/' + CAST(YEAR(GETDATE()) AS VARCHAR)
        WHERE dro_id = @dro_id_draft;
    END
    
    -- Return ID yang baru dibuat atau ID yang sama (jika sudah format DO)
    SELECT TOP 1 dro_id 
    FROM sia_msdropout 
    WHERE dro_id = @dro_id_draft 
       OR dro_id = (
           SELECT TOP 1 dro_id 
           FROM sia_msdropout 
           WHERE dro_id LIKE '%DO%'
           ORDER BY dro_created_date DESC
       )
    ORDER BY dro_created_date DESC;
END
GO

-- =============================================
-- Test Script
-- =============================================

PRINT '========================================='
PRINT 'TEST: Validasi Status'
PRINT '========================================='

-- Test 1: Draft (BOLEH)
PRINT 'Test 1: Draft -> BOLEH'
-- EXEC sia_getIdDOByDraft @dro_id_draft = 'DRAFT-TEST-001'

-- Test 2: Ditolak (BOLEH)
PRINT 'Test 2: Ditolak -> BOLEH'
-- UPDATE sia_msdropout SET dro_status = 'Ditolak' WHERE dro_id = '16/PMA/DO/I/2026'
-- EXEC sia_getIdDOByDraft @dro_id_draft = '16/PMA/DO/I/2026'

-- Test 3: Belum Disetujui Wadir 1 (TIDAK BOLEH)
PRINT 'Test 3: Belum Disetujui Wadir 1 -> TIDAK BOLEH'
-- UPDATE sia_msdropout SET dro_status = 'Belum Disetujui Wadir 1' WHERE dro_id = '16/PMA/DO/I/2026'
-- EXEC sia_getIdDOByDraft @dro_id_draft = '16/PMA/DO/I/2026'
-- Expected: ERROR: Drop Out dengan status 'Belum Disetujui Wadir 1' tidak dapat diajukan ulang

-- Test 4: Disetujui Wadir 1 (TIDAK BOLEH)
PRINT 'Test 4: Disetujui Wadir 1 -> TIDAK BOLEH'
-- UPDATE sia_msdropout SET dro_status = 'Disetujui Wadir 1' WHERE dro_id = '16/PMA/DO/I/2026'
-- EXEC sia_getIdDOByDraft @dro_id_draft = '16/PMA/DO/I/2026'
-- Expected: ERROR: Drop Out dengan status 'Disetujui Wadir 1' tidak dapat diajukan ulang

-- Test 5: Selesai (TIDAK BOLEH)
PRINT 'Test 5: Selesai -> TIDAK BOLEH'
-- UPDATE sia_msdropout SET dro_status = 'Selesai' WHERE dro_id = '16/PMA/DO/I/2026'
-- EXEC sia_getIdDOByDraft @dro_id_draft = '16/PMA/DO/I/2026'
-- Expected: ERROR: Drop Out dengan status 'Selesai' tidak dapat diajukan ulang

PRINT '========================================='
PRINT 'Uncomment test yang ingin dijalankan'
PRINT '========================================='
