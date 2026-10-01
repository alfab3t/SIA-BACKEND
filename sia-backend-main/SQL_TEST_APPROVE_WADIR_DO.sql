-- Test SP approve Wadir untuk DropOut
-- Ganti dro_id dan username dengan nilai yang sesuai

EXEC sia_approveDropOut 
    @dro_id = '1/PMA/DO/XII/2025',
    @username = 'admin';

-- Cek hasilnya
SELECT 
    dro_id,
    dro_status,
    dro_approval_dir1_by,
    dro_app_dir1_date,
    srt_no
FROM sia_msdropout
WHERE dro_id = '1/PMA/DO/XII/2025';
