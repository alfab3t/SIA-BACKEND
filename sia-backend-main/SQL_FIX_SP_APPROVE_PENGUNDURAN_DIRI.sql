    -- =============================================
    -- Fix SP sia_setujuiPengunduranDiri
    -- Problem: sia_createNoSurat expects 4 params: @JenisSuratId, @DosenId, @KonsentrasiId, @CreatedBy + OUTPUT
    -- Solution: Match parameter signature correctly
    -- =============================================

    USE [ERP_PolmanAstra_NDA]
    GO

    ALTER PROCEDURE [dbo].[sia_setujuiPengunduranDiri]
        @pdi_id VARCHAR(MAX),
        @role VARCHAR(MAX),
        @approved_by VARCHAR(MAX)
    AS
    BEGIN
        SET NOCOUNT ON;

        -- Approve by Prodi
        IF (@role = 'Prodi')
        BEGIN
            UPDATE sia_mspengundurandiri
            SET pdi_approval_prodi_by = @approved_by,
                pdi_status = 'Belum Disetujui Wadir 1',
                pdi_app_prodi_date = GETDATE()
            WHERE pdi_id = @pdi_id;
        END

    -- Approve by Wadir1 (generates nomor surat)
    IF (@role = 'Wadir1')
    BEGIN
        DECLARE @srtno VARCHAR(30);
        DECLARE @srtno2 VARCHAR(30);
        DECLARE @kaprodi VARCHAR(50);
        DECLARE @kaprodi_dsn_id VARCHAR(50);
        DECLARE @jsu_id INT;
        DECLARE @kon_id INT;

        -- Get kaprodi and konsentrasi from existing data
        SELECT @kaprodi = pdi_approval_prodi_by, @kon_id = kon_id 
        FROM sia_mspengundurandiri a
        INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id 
        WHERE pdi_id = @pdi_id;

        -- Convert kaprodi username to dsn_id (sia_createNoSurat needs dsn_id not username)
        SELECT @kaprodi_dsn_id = CAST(dsn_id AS VARCHAR(50))
        FROM sia_msdosen
        WHERE dsn_namaakun = @kaprodi;

        -- Get jenis surat ID for SK Pengunduran Diri
        SELECT @jsu_id = jsu_id 
        FROM sia_msjenissurat 
        WHERE jsu_nama_surat = 'Surat Keputusan Pengunduran Diri';

        -- Generate nomor surat SK (FIXED: use dsn_id instead of username)
        -- sia_createNoSurat expects: @JenisSuratId, @DosenId, @KonsentrasiId, @CreatedBy, @NomorSurat OUTPUT
        EXEC sia_createNoSurat @jsu_id, @kaprodi_dsn_id, @kon_id, @approved_by, @srtno OUTPUT;
        EXEC sia_createNoSurat '5', @kaprodi_dsn_id, @kon_id, @approved_by, @srtno2 OUTPUT;PUT;

            -- Update with approval and generated nomor surat
            UPDATE sia_mspengundurandiri
            SET srt_no = @srtno,
                pdi_no_skpb = @srtno2,
                pdi_approval_dir1_by = @approved_by,
                pdi_status = 'Menunggu Upload SK',
                pdi_app_dir1_date = GETDATE()
            WHERE pdi_id = @pdi_id;
        END
    END
    GO

    PRINT 'SP sia_setujuiPengunduranDiri updated - now matches sia_createNoSurat signature (4 input + 1 output)'
