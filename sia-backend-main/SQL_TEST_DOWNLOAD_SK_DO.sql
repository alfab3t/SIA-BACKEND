-- Test SP download SK untuk DropOut
-- Ganti dro_id dengan ID yang sesuai

EXEC sia_downloadSKDO @dro_id = '1/PMA/DO/XII/2025';

-- Atau cek langsung dari table
SELECT 
    dro_id,
    dro_sk,
    dro_skpb,
    dro_status
FROM sia_msdropout
WHERE dro_id = '1/PMA/DO/XII/2025';
