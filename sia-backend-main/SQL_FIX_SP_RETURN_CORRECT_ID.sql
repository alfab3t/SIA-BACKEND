USE [ERP_PolmanAstra_NDA]
GO

ALTER PROCEDURE [dbo].[sia_getIdDOByDraft]
    @dro_id_draft VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @tempIdDO INT;
    DECLARE @tempRomawi VARCHAR(5);
    DECLARE @finalId VARCHAR(50);  -- Variable to store the final ID to return

    -- Update tanggal created dan status menjadi "Belum Disetujui Wadir 1"
    UPDATE sia_msdropout
    SET dro_created_date = GETDATE(),
        dro_status = 'Belum Disetujui Wadir 1'
    WHERE dro_id = @dro_id_draft;

    -- Check if the input ID already has DO format
    IF @dro_id_draft LIKE '%DO%'
    BEGIN
        -- Already has DO format, just return the same ID
        SET @finalId = @dro_id_draft;
    END
    ELSE
    BEGIN
        -- Still draft format, generate new DO ID
        
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

        -- Generate new ID with format: {nomor}/PMA/DO/{bulan_romawi}/{tahun}
        SET @finalId = CAST(@tempIdDO AS VARCHAR) + '/PMA/DO/' + @tempRomawi + '/' + CAST(YEAR(GETDATE()) AS VARCHAR);
        
        -- Update the record with new ID
        UPDATE sia_msdropout
        SET dro_id = @finalId
        WHERE dro_id = @dro_id_draft;
    END

    -- Return the final ID (either the same DO ID or the newly generated one)
    SELECT @finalId AS dro_id;
END
GO
