-- =============================================
-- Debug Approve Pengunduran Diri
-- Cek kenapa approve gagal
-- =============================================

-- 1. Cek data yang akan di-approve
DECLARE @pdi_id VARCHAR(50) = '1'; -- GANTI DENGAN PDI_ID YANG MAU DI-APPROVE

SELECT 
    pdi_id,
    mhs_id,
    pdi_status,
    pdi_approval_prodi_by,
    pdi_app_prodi_date,
    pdi_approval_dir1_by,
    pdi_app_dir1_date,
    srt_no,
    pdi_no_skpb,
    pdi_created_date,
    pdi_modif_date
FROM sia_mspengundurandiri
WHERE pdi_id = @pdi_id;

-- 2. Cek struktur table sia_mspengundurandiri
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'sia_mspengundurandiri'
AND COLUMN_NAME IN (
    'pdi_id',
    'pdi_status',
    'pdi_approval_prodi_by',
    'pdi_app_prodi_date',
    'pdi_approval_dir1_by',
    'pdi_app_dir1_date',
    'srt_no',
    'pdi_no_skpb',
    'pdi_alasan_tolak'
)
ORDER BY ORDINAL_POSITION;

-- 3. Cek apakah SP sia_createNoSurat ada
SELECT 
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_NAME = 'sia_createNoSurat';

-- 4. Cek jenis surat untuk Pengunduran Diri
SELECT * FROM sia_msjenissurat 
WHERE jsu_nama_surat LIKE '%Pengunduran%';

-- 5. Test approve Prodi (dry run)
DECLARE @test_pdi_id VARCHAR(50) = '1'; -- GANTI
DECLARE @test_role VARCHAR(50) = 'Prodi';
DECLARE @test_approved_by VARCHAR(50) = 'nda_prodi'; -- GANTI

PRINT 'Testing Prodi Approve...';
PRINT 'PDI_ID: ' + @test_pdi_id;
PRINT 'Role: ' + @test_role;
PRINT 'Approved By: ' + @test_approved_by;

-- Cek status sebelum approve
SELECT 'BEFORE APPROVE' AS stage, pdi_id, pdi_status, pdi_approval_prodi_by
FROM sia_mspengundurandiri
WHERE pdi_id = @test_pdi_id;

-- Uncomment untuk test approve
/*
EXEC sia_setujuiPengunduranDiri 
    @pdi_id = @test_pdi_id,
    @role = @test_role,
    @approved_by = @test_approved_by;

-- Cek status setelah approve
SELECT 'AFTER APPROVE' AS stage, pdi_id, pdi_status, pdi_approval_prodi_by, pdi_app_prodi_date
FROM sia_mspengundurandiri
WHERE pdi_id = @test_pdi_id;
*/

-- 6. Cek error log jika ada
SELECT TOP 10 *
FROM sys.messages
WHERE language_id = 1033
ORDER BY message_id DESC;
