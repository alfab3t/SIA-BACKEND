-- Check for dropout records without matching mahasiswa data
USE [ERP_PolmanAstra_NDA]
GO

-- Find dropout records that don't have matching mahasiswa
SELECT 
    a.dro_id,
    a.mhs_id,
    a.dro_status,
    a.dro_created_date,
    CASE 
        WHEN b.mhs_id IS NULL THEN 'NO MAHASISWA DATA'
        WHEN c.kon_id IS NULL THEN 'NO KONSENTRASI DATA'
        WHEN d.pro_id IS NULL THEN 'NO PRODI DATA'
        ELSE 'OK'
    END AS data_status
FROM sia_msdropout a
LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
LEFT JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
LEFT JOIN sia_msprodi d ON d.pro_id = c.pro_id
WHERE b.mhs_id IS NULL 
   OR c.kon_id IS NULL 
   OR d.pro_id IS NULL
ORDER BY a.dro_created_date DESC;

-- Check specific records
SELECT 
    'Dropout Record' AS source,
    dro_id,
    mhs_id,
    dro_status,
    dro_created_date
FROM sia_msdropout
WHERE dro_id IN ('1', '2')
ORDER BY dro_created_date DESC;

-- Check if these mhs_id exist in mahasiswa table
SELECT 
    'Mahasiswa Record' AS source,
    mhs_id,
    mhs_nama,
    kon_id
FROM sia_msmahasiswa
WHERE mhs_id IN (
    SELECT mhs_id FROM sia_msdropout WHERE dro_id IN ('1', '2')
);
