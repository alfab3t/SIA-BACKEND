-- =============================================
-- FIX SP: sia_detailDO
-- Add missing columns: srt_no, dro_skpb, dro_created_date, dro_modif_by, dro_modif_date
-- These columns are needed by GetByIdAsync method in C# code
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

ALTER PROCEDURE [dbo].[sia_detailDO]
    @dro_id VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        dro_id,
        a.mhs_id,
        a.mhs_id + ' - ' + mhs_nama AS mhstext,
        pro_nama + ' (' + kon_singkatan + ')' AS kon_nama,
        mhs_angkatan,
        dro_menimbang,
        dro_mengingat,
        dro_status,
        dro_created_by,
        dro_sk,
        dro_skpb,                                                    -- NEW: Added
        srt_no,                                                      -- NEW: Added
        CONVERT(VARCHAR(11), dro_appr_wadir1_date, 106) AS dro_appr_wadir1_date,
        dro_appr_wadir1,
        CONVERT(VARCHAR(11), dro_appr_dir_date, 106) AS dro_appr_dir_date,
        dro_appr_dir,
        dro_alasan_tolak,
        kon_nama AS kon_nama2,
        pro_nama,
        dro_srt_ket_no,
        dro_created_date,                                            -- NEW: Added
        dro_modif_by,                                                -- NEW: Added
        dro_modif_date                                               -- NEW: Added
    FROM sia_msdropout a
    INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
    INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
    WHERE a.dro_id = @dro_id;
END
GO

PRINT 'SP sia_detailDO updated - Added missing columns: srt_no, dro_skpb, dro_created_date, dro_modif_by, dro_modif_date'
GO

-- Test the SP
PRINT 'Testing SP sia_detailDO...'
GO

-- You can test with an actual dro_id from your database
-- EXEC sia_detailDO @dro_id = 'YOUR_DRO_ID_HERE'
GO
