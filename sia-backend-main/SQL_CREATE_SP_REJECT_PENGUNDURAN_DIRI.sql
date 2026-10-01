-- =============================================
-- Create SP: sia_tolakPengunduranDiri
-- Reject Pengunduran Diri (Prodi atau Wadir1)
-- =============================================
USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sia_tolakPengunduranDiri]
    @pdi_id VARCHAR(50),
    @role VARCHAR(50),
    @alasan_tolak VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @current_status VARCHAR(50);
    DECLARE @new_status VARCHAR(50);
    
    -- Get current status
    SELECT @current_status = pdi_status 
    FROM sia_mspengundurandiri 
    WHERE pdi_id = @pdi_id;
    
    -- Validate data exists
    IF @current_status IS NULL
    BEGIN
        RAISERROR('Data pengunduran diri tidak ditemukan', 16, 1);
        RETURN;
    END
    
    -- Determine new status based on role
    IF @role = 'Prodi'
    BEGIN
        -- Prodi reject: change status to "Ditolak Prodi"
        IF @current_status = 'Belum Disetujui Prodi'
        BEGIN
            SET @new_status = 'Ditolak Prodi';
            
            UPDATE sia_mspengundurandiri
            SET pdi_status = @new_status,
                pdi_alasan_tolak = @alasan_tolak,
                pdi_modif_date = GETDATE()
            WHERE pdi_id = @pdi_id;
        END
        ELSE
        BEGIN
            RAISERROR('Status tidak valid untuk reject Prodi. Status saat ini: %s', 16, 1, @current_status);
            RETURN;
        END
    END
    ELSE IF @role = 'Wadir1'
    BEGIN
        -- Wadir1 reject: change status to "Ditolak Wadir 1"
        IF @current_status = 'Belum Disetujui Wadir 1'
        BEGIN
            SET @new_status = 'Ditolak Wadir 1';
            
            UPDATE sia_mspengundurandiri
            SET pdi_status = @new_status,
                pdi_alasan_tolak = @alasan_tolak,
                pdi_modif_date = GETDATE()
            WHERE pdi_id = @pdi_id;
        END
        ELSE
        BEGIN
            RAISERROR('Status tidak valid untuk reject Wadir1. Status saat ini: %s', 16, 1, @current_status);
            RETURN;
        END
    END
    ELSE
    BEGIN
        RAISERROR('Role tidak valid: %s. Harus Prodi atau Wadir1', 16, 1, @role);
        RETURN;
    END
    
    -- Return success message
    SELECT 
        @pdi_id AS pdi_id,
        @new_status AS new_status,
        @alasan_tolak AS alasan_tolak,
        GETDATE() AS rejected_date,
        'Success' AS result;
END
GO
