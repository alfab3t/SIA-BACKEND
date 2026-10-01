-- =============================================
-- Create SP: sia_setujuiPengunduranDiri
-- Approve Pengunduran Diri (Prodi atau Wadir1)
-- =============================================
USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sia_setujuiPengunduranDiri]
    @pdi_id VARCHAR(50),
    @role VARCHAR(50),
    @approved_by VARCHAR(50)
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
        -- Prodi approve: change status to "Belum Disetujui Wadir 1"
        IF @current_status = 'Belum Disetujui Prodi'
        BEGIN
            SET @new_status = 'Belum Disetujui Wadir 1';
            
            UPDATE sia_mspengundurandiri
            SET pdi_status = @new_status,
                pdi_approval_prodi_by = @approved_by,
                pdi_app_prodi_date = GETDATE(),
                pdi_modif_by = @approved_by,
                pdi_modif_date = GETDATE()
            WHERE pdi_id = @pdi_id;
        END
        ELSE
        BEGIN
            RAISERROR('Status tidak valid untuk approval Prodi. Status saat ini: %s', 16, 1, @current_status);
            RETURN;
        END
    END
    ELSE IF @role = 'Wadir1'
    BEGIN
        -- Wadir1 approve: change status to "Disetujui"
        IF @current_status = 'Belum Disetujui Wadir 1'
        BEGIN
            SET @new_status = 'Disetujui';
            
            UPDATE sia_mspengundurandiri
            SET pdi_status = @new_status,
                pdi_approval_dir1_by = @approved_by,
                pdi_app_dir1_date = GETDATE(),
                pdi_modif_by = @approved_by,
                pdi_modif_date = GETDATE()
            WHERE pdi_id = @pdi_id;
        END
        ELSE
        BEGIN
            RAISERROR('Status tidak valid untuk approval Wadir1. Status saat ini: %s', 16, 1, @current_status);
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
        @approved_by AS approved_by,
        GETDATE() AS approved_date,
        'Success' AS result;
END
GO
