-- =============================================
-- Direct test of Mode 4 logic to see what's missing
-- =============================================

USE [ERP_PolmanAstra_NDA]
GO

PRINT '=== TESTING MODE 4 LOGIC DIRECTLY ==='
PRINT ''

-- Test the exact query that Mode 4 should execute
PRINT '1. Testing Mode 4 query without status filter:'
SELECT COUNT(*) as total_without_status_filter
FROM sia_mspengundurandiri a
LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER('') + '%' 
       OR UPPER(b.mhs_nama) LIKE '%' + UPPER('') + '%' 
       OR UPPER(a.srt_no) LIKE '%' + UPPER('') + '%')
AND b.kon_id = (CASE WHEN '' = '' THEN b.kon_id ELSE '' END);

PRINT ''
PRINT '2. Testing Mode 4 query WITH status filter:'
SELECT COUNT(*) as total_with_status_filter
FROM sia_mspengundurandiri a
LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
WHERE (UPPER(a.mhs_id) LIKE '%' + UPPER('') + '%' 
       OR UPPER(b.mhs_nama) LIKE '%' + UPPER('') + '%' 
       OR UPPER(a.srt_no) LIKE '%' + UPPER('') + '%')
AND b.kon_id = (CASE WHEN '' = '' THEN b.kon_id ELSE '' END)
AND a.pdi_status IN ('Disetujui','Ditolak Prodi','Ditolak Wadir1');

PRINT ''
PRINT '3. Check if all tables have data and relationships:'
SELECT 
    'sia_mspengundurandiri' as table_name,
    COUNT(*) as record_count
FROM sia_mspengundurandiri
UNION ALL
SELECT 
    'sia_msmahasiswa' as table_name,
    COUNT(*) as record_count
FROM sia_msmahasiswa
UNION ALL
SELECT 
    'sia_mskonsentrasi' as table_name,
    COUNT(*) as record_count
FROM sia_mskonsentrasi
UNION ALL
SELECT 
    'sia_msprodi' as table_name,
    COUNT(*) as record_count
FROM sia_msprodi;

PRINT ''
PRINT '4. Check for missing relationships:'
-- Check pengunduran diri records without mahasiswa
SELECT COUNT(*) as pengunduran_without_mahasiswa
FROM sia_mspengundurandiri a
LEFT JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
WHERE b.mhs_id IS NULL;

-- Check mahasiswa without konsentrasi
SELECT COUNT(*) as mahasiswa_without_konsentrasi
FROM sia_msmahasiswa b
LEFT JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
WHERE z.kon_id IS NULL;

-- Check konsentrasi without prodi
SELECT COUNT(*) as konsentrasi_without_prodi
FROM sia_mskonsentrasi z
LEFT JOIN sia_msprodi d ON d.pro_id = z.pro_id
WHERE d.pro_id IS NULL;

PRINT ''
PRINT '5. Sample records with all joins:'
SELECT TOP 5
    a.pdi_id,
    a.mhs_id,
    a.pdi_status,
    b.mhs_nama,
    z.kon_singkatan,
    d.pro_nama
FROM sia_mspengundurandiri a
LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
WHERE a.pdi_status IN ('Disetujui','Ditolak Prodi','Ditolak Wadir1')
ORDER BY a.pdi_created_date DESC;

PRINT ''
PRINT '6. Test the exact dynamic SQL that should be generated:'
DECLARE @sql NVARCHAR(MAX) = '
SELECT a.pdi_id,
    a.mhs_id,
    a.pdi_approval_prodi_by AS approve_prodi,
    a.pdi_approval_dir1_by AS approve_dir1,
    CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS tanggal, 
    CONVERT(VARCHAR(11), c.srt_created_date, 106) AS tanggal_disetujui,
    a.srt_no,
    mhs_nama,
    d.pro_nama AS prodi_nama,
    z.kon_singkatan AS konsentrasi,
    pdi_status AS status
FROM sia_mspengundurandiri a
LEFT JOIN sia_mssurat c ON a.srt_no = c.srt_no
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi z ON b.kon_id = z.kon_id
INNER JOIN sia_msprodi d ON d.pro_id = z.pro_id
WHERE (UPPER(a.mhs_id) LIKE ''%'' + UPPER('''') + ''%'' 
       OR UPPER(b.mhs_nama) LIKE ''%'' + UPPER('''') + ''%'' 
       OR UPPER(a.srt_no) LIKE ''%'' + UPPER('''') + ''%'')
AND b.kon_id = (CASE WHEN '''' = '''' THEN b.kon_id ELSE '''' END)
AND a.pdi_status IN (''Disetujui'',''Ditolak Prodi'',''Ditolak Wadir1'')
ORDER BY a.pdi_created_date DESC
OFFSET 0 ROWS
FETCH NEXT 10 ROWS ONLY';

PRINT 'Executing dynamic SQL:'
PRINT @sql
EXEC(@sql);

PRINT ''
PRINT '=== MODE 4 DIRECT TEST COMPLETE ==='