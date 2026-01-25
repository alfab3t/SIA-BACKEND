-- =============================================
-- Script untuk membuat user admin baru
-- Tanpa perlu LDAP, langsung bisa login
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- 1. Insert user baru ke tabel sso_msuser (atau tabel user yang sesuai)
-- Ganti 'adminsia' dengan username yang diinginkan
DECLARE @Username VARCHAR(50) = 'adminsia'
DECLARE @NamaLengkap VARCHAR(100) = 'Admin SIA'

-- Cek apakah user sudah ada
IF NOT EXISTS (SELECT 1 FROM sso_msuser WHERE usr_username = @Username)
BEGIN
    INSERT INTO sso_msuser (usr_username, usr_nama, usr_status, usr_created_date)
    VALUES (@Username, @NamaLengkap, 1, GETDATE())
    
    PRINT 'User ' + @Username + ' berhasil dibuat'
END
ELSE
BEGIN
    PRINT 'User ' + @Username + ' sudah ada'
END

-- 2. Berikan akses ke aplikasi SIA
-- Cari app_id untuk aplikasi SIA
DECLARE @AppId VARCHAR(50)
SELECT TOP 1 @AppId = app_id FROM sso_msaplikasi WHERE app_nama LIKE '%SIA%' OR app_deskripsi LIKE '%SIA%'

IF @AppId IS NULL
BEGIN
    PRINT 'ERROR: Aplikasi SIA tidak ditemukan. Cek tabel sso_msaplikasi'
    -- Tampilkan daftar aplikasi yang ada
    SELECT app_id, app_nama, app_deskripsi FROM sso_msaplikasi
END
ELSE
BEGIN
    PRINT 'App ID SIA: ' + @AppId
    
    -- 3. Cari atau buat role admin
    DECLARE @RoleId VARCHAR(50)
    SELECT TOP 1 @RoleId = rol_id 
    FROM sso_msrole 
    WHERE rol_nama LIKE '%admin%' OR rol_deskripsi LIKE '%admin%'
    
    IF @RoleId IS NULL
    BEGIN
        -- Buat role admin baru jika belum ada
        SET @RoleId = NEWID()
        INSERT INTO sso_msrole (rol_id, rol_nama, rol_deskripsi, rol_status)
        VALUES (@RoleId, 'Admin', 'Administrator', 1)
        PRINT 'Role Admin dibuat dengan ID: ' + @RoleId
    END
    ELSE
    BEGIN
        PRINT 'Role ID Admin: ' + @RoleId
    END
    
    -- 4. Assign user ke aplikasi dengan role admin
    IF NOT EXISTS (
        SELECT 1 FROM sso_msuserrole 
        WHERE usr_username = @Username 
        AND app_id = @AppId 
        AND rol_id = @RoleId
    )
    BEGIN
        INSERT INTO sso_msuserrole (usr_username, app_id, rol_id, usr_status)
        VALUES (@Username, @AppId, @RoleId, 1)
        PRINT 'User ' + @Username + ' berhasil di-assign ke aplikasi SIA sebagai Admin'
    END
    ELSE
    BEGIN
        PRINT 'User ' + @Username + ' sudah memiliki akses ke aplikasi SIA'
    END
    
    -- 5. Berikan semua permission ke role admin
    -- Insert semua menu yang ada ke role admin
    INSERT INTO sso_msrolemenu (rol_id, men_id, app_id)
    SELECT @RoleId, men_id, @AppId
    FROM sso_msmenu
    WHERE app_id = @AppId
    AND NOT EXISTS (
        SELECT 1 FROM sso_msrolemenu 
        WHERE rol_id = @RoleId 
        AND men_id = sso_msmenu.men_id
        AND app_id = @AppId
    )
    
    PRINT 'Semua menu berhasil di-assign ke role Admin'
END

-- 6. Tampilkan hasil
PRINT ''
PRINT '========================================='
PRINT 'INFORMASI LOGIN:'
PRINT '========================================='
PRINT 'Username: ' + @Username
PRINT 'Password: (gunakan password LDAP yang sama, atau buat bypass di kode)'
PRINT 'Nama: ' + @NamaLengkap
PRINT ''
PRINT 'User ini sekarang memiliki akses penuh ke aplikasi SIA'
PRINT '========================================='

-- Verifikasi data yang baru dibuat
SELECT 
    u.usr_username,
    u.usr_nama,
    a.app_nama,
    a.app_deskripsi,
    r.rol_nama,
    r.rol_deskripsi
FROM sso_msuser u
INNER JOIN sso_msuserrole ur ON u.usr_username = ur.usr_username
INNER JOIN sso_msaplikasi a ON ur.app_id = a.app_id
INNER JOIN sso_msrole r ON ur.rol_id = r.rol_id
WHERE u.usr_username = @Username

GO
