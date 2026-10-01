-- =============================================
-- Check and Add drop_out.view Permission
-- This permission is required for GET /api/DropOut/mahasiswa endpoint
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

-- Check if permission exists
SELECT 
    p.per_id,
    p.per_name,
    p.per_description,
    rp.rol_id,
    r.rol_name
FROM sia_mspermission p
LEFT JOIN sia_msrolepermission rp ON p.per_id = rp.per_id
LEFT JOIN sia_msrole r ON rp.rol_id = r.rol_id
WHERE p.per_name = 'drop_out.view'
ORDER BY r.rol_name;

-- If permission doesn't exist, create it
IF NOT EXISTS (SELECT 1 FROM sia_mspermission WHERE per_name = 'drop_out.view')
BEGIN
    INSERT INTO sia_mspermission (per_name, per_description)
    VALUES ('drop_out.view', 'View Drop Out data and mahasiswa list');
    
    PRINT 'Permission drop_out.view has been created';
END
ELSE
BEGIN
    PRINT 'Permission drop_out.view already exists';
END

-- Check which roles have this permission
PRINT '';
PRINT 'Roles with drop_out.view permission:';
SELECT 
    r.rol_id,
    r.rol_name,
    r.rol_description
FROM sia_msrole r
INNER JOIN sia_msrolepermission rp ON r.rol_id = rp.rol_id
INNER JOIN sia_mspermission p ON rp.per_id = p.per_id
WHERE p.per_name = 'drop_out.view';

-- To add permission to a specific role, uncomment and modify:
/*
DECLARE @per_id INT;
DECLARE @rol_id INT;

SELECT @per_id = per_id FROM sia_mspermission WHERE per_name = 'drop_out.view';
SELECT @rol_id = rol_id FROM sia_msrole WHERE rol_name = 'Prodi'; -- Change role name as needed

IF NOT EXISTS (SELECT 1 FROM sia_msrolepermission WHERE per_id = @per_id AND rol_id = @rol_id)
BEGIN
    INSERT INTO sia_msrolepermission (per_id, rol_id)
    VALUES (@per_id, @rol_id);
    
    PRINT 'Permission drop_out.view has been added to role';
END
*/
