-- =============================================
-- Check dan Fix Permission Delete Pengunduran Diri
-- =============================================

-- 1. Cek apakah permission delete sudah ada
SELECT * FROM sso_mspermission 
WHERE per_name = 'pengunduran_diri.delete';

-- 2. Jika belum ada, insert permission delete
-- (Skip jika sudah ada)
IF NOT EXISTS (SELECT 1 FROM sso_mspermission WHERE per_name = 'pengunduran_diri.delete')
BEGIN
    INSERT INTO sso_mspermission (per_id, per_name, per_description, per_status)
    VALUES ('PER_PD_DELETE', 'pengunduran_diri.delete', 'Delete Pengunduran Diri', 'Aktif');
    PRINT 'Permission pengunduran_diri.delete berhasil ditambahkan';
END
ELSE
BEGIN
    PRINT 'Permission pengunduran_diri.delete sudah ada';
END

-- 3. Cek user yang sedang login (ganti dengan username Anda)
DECLARE @username VARCHAR(50) = 'nda_prodi'; -- GANTI DENGAN USERNAME ANDA

-- 4. Cek role user
SELECT 
    u.usr_username,
    u.usr_id,
    r.rol_id,
    r.rol_name
FROM sso_msuser u
INNER JOIN sso_msuserrole ur ON u.usr_id = ur.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
WHERE u.usr_username = @username;

-- 5. Cek permission yang dimiliki user untuk Pengunduran Diri
SELECT 
    u.usr_username,
    r.rol_name,
    p.per_name,
    p.per_description
FROM sso_msuser u
INNER JOIN sso_msuserrole ur ON u.usr_id = ur.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
INNER JOIN sso_msrolepermission rp ON r.rol_id = rp.rol_id
INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
WHERE u.usr_username = @username
AND p.per_name LIKE 'pengunduran_diri.%'
ORDER BY p.per_name;

-- 6. Tambahkan permission delete ke role user (jika belum ada)
-- Contoh: Tambahkan ke role Admin
DECLARE @rol_id VARCHAR(50);
SELECT @rol_id = r.rol_id 
FROM sso_msuser u
INNER JOIN sso_msuserrole ur ON u.usr_id = ur.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
WHERE u.usr_username = @username;

IF @rol_id IS NOT NULL
BEGIN
    -- Cek apakah role sudah punya permission delete
    IF NOT EXISTS (
        SELECT 1 FROM sso_msrolepermission rp
        INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
        WHERE rp.rol_id = @rol_id 
        AND p.per_name = 'pengunduran_diri.delete'
    )
    BEGIN
        -- Tambahkan permission delete ke role
        INSERT INTO sso_msrolepermission (rol_id, per_id)
        SELECT @rol_id, per_id 
        FROM sso_mspermission 
        WHERE per_name = 'pengunduran_diri.delete';
        
        PRINT 'Permission pengunduran_diri.delete berhasil ditambahkan ke role ' + @rol_id;
    END
    ELSE
    BEGIN
        PRINT 'Role ' + @rol_id + ' sudah memiliki permission pengunduran_diri.delete';
    END
END
ELSE
BEGIN
    PRINT 'User tidak ditemukan atau tidak memiliki role';
END

-- 7. Verifikasi permission setelah ditambahkan
SELECT 
    u.usr_username,
    r.rol_name,
    p.per_name,
    p.per_description
FROM sso_msuser u
INNER JOIN sso_msuserrole ur ON u.usr_id = ur.usr_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
INNER JOIN sso_msrolepermission rp ON r.rol_id = rp.rol_id
INNER JOIN sso_mspermission p ON rp.per_id = p.per_id
WHERE u.usr_username = @username
AND p.per_name = 'pengunduran_diri.delete';
