-- =============================================
-- Check str_main_id for user nda_prodi
-- Debug query to understand user role and permissions
-- =============================================

-- 1. Check user details in ess_mskaryawan
SELECT 
    'USER DETAILS' as section,
    kry_username,
    kry_id,
    str_main_id,
    kry_nama
FROM ess_mskaryawan 
WHERE kry_username = 'nda_prodi';

-- 2. Check if nda_prodi exists as mahasiswa
SELECT 
    'MAHASISWA CHECK' as section,
    COUNT(*) as is_mahasiswa
FROM sia_msmahasiswa 
WHERE mhs_id = 'nda_prodi';

-- 3. Check konsentrasi where this user is sekprodi
SELECT 
    'KONSENTRASI SEKPRODI' as section,
    kon_id,
    kon_nama,
    kon_sekprodi,
    pro_id
FROM sia_mskonsentrasi 
WHERE kon_sekprodi = (SELECT kry_id FROM ess_mskaryawan WHERE kry_username = 'nda_prodi');

-- 4. Check sample data in pengunduran diri table
SELECT TOP 10
    'SAMPLE DATA' as section,
    pdi_id,
    mhs_id,
    pdi_status,
    pdi_created_date
FROM sia_mspengundurandiri
ORDER BY pdi_created_date DESC;

-- 5. Check mahasiswa data with konsentrasi info
SELECT TOP 5
    'MAHASISWA WITH KONSENTRASI' as section,
    b.mhs_id,
    b.mhs_nama,
    c.kon_id,
    c.kon_nama,
    c.kon_sekprodi,
    d.pro_id,
    d.pro_nama
FROM sia_msmahasiswa b
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
WHERE EXISTS (SELECT 1 FROM sia_mspengundurandiri a WHERE a.mhs_id = b.mhs_id);

-- 6. Test role-based access for nda_prodi
DECLARE @username VARCHAR(50) = 'nda_prodi';
DECLARE @str VARCHAR(MAX);
DECLARE @kryid VARCHAR(MAX);

SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
SELECT @kryid = kry_id FROM ess_mskaryawan WHERE kry_username = @username;

SELECT 
    'ROLE ANALYSIS' as section,
    @username as username,
    @str as str_main_id,
    @kryid as kry_id,
    CASE 
        WHEN EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username) THEN 'MAHASISWA'
        WHEN @str = '2' THEN 'WADIR 1'
        WHEN @str = '1' THEN 'DIREKTUR'
        WHEN @str IN ('14', '54', '26', '19') THEN 'ADMIN/STAFF'
        WHEN @str IN ('27', '23', '28') THEN 'STAFF UPLOAD SK'
        ELSE 'SEKPRODI'
    END as role_type;