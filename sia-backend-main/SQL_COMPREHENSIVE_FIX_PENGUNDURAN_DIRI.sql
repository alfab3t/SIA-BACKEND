-- Cek str_main_id untuk user nda_prodi
SELECT 
    kry_username,
    kry_id,
    str_main_id,
    kry_nama
FROM ess_mskaryawan 
WHERE kry_username = 'nda_prodi';

-- Cek apakah nda_prodi adalah mahasiswa
SELECT COUNT(*) as is_mahasiswa
FROM sia_msmahasiswa 
WHERE mhs_id = 'nda_prodi';

-- Cek data pengunduran diri yang ada
SELECT TOP 5
    pdi_id,
    mhs_id,
    pdi_status,
    pdi_created_date
FROM sia_mspengundurandiri
ORDER BY pdi_created_date DESC;
GO