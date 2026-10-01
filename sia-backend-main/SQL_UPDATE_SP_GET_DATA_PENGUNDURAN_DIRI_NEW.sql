-- =============================================
-- Update SP: sia_getDataPengunduranDiri
-- Support: Mahasiswa, Role-based access, Multiple status, Konsentrasi filter, Keyword search
-- EXACT COPY dari pattern sia_getDataPendingDO
-- =============================================
USE [ERP_PolmanAstra_NDA]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sia_getDataPengunduranDiri]
@username VARCHAR(50),
@keyword VARCHAR(MAX),
@sort_by VARCHAR(100),
@kon_id VARCHAR(50),
@pdi_status VARCHAR(50),
@kry_id VARCHAR(50),
@status VARCHAR(MAX) = '',
@Page INT = NULL,
@PageSize INT = NULL
AS
BEGIN
SET NOCOUNT ON;

DECLARE @SQL NVARCHAR(MAX);
DECLARE @viewBy NVARCHAR(MAX);
DECLARE @statusFilter NVARCHAR(MAX) = '';
DECLARE @str VARCHAR(MAX);
DECLARE @kryid VARCHAR(MAX);
DECLARE @kons VARCHAR(MAX);

-- Ambil struktur organisasi dan karyawan ID
SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;
SELECT @kryid = kry_id FROM ess_mskaryawan WHERE kry_username = @username;
SELECT @kons = kon_id FROM sia_mskonsentrasi WHERE kon_sekprodi = @kryid;

    -- Logic filter berdasarkan role/struktur untuk PENGAJUAN PENGUNDURAN DIRI
    IF EXISTS (SELECT 1 FROM sia_msmahasiswa WHERE mhs_id = @username)
    BEGIN
        -- Mahasiswa: hanya lihat data sendiri
        SET @viewBy = ' AND a.mhs_id = ''' + @username + '''';
    END
    ELSE IF @str = '2' 
    BEGIN
        -- Wadir 1: lihat yang perlu approval Wadir 1
        SET @viewBy = ' AND a.pdi_status IN (''Belum Disetujui Wadir 1'')';
    END 
    ELSE IF @str = '1' 
    BEGIN
        -- Direktur: lihat yang perlu approval Direktur dan status lainnya
        SET @viewBy = ' AND a.pdi_status IN (''Belum Disetujui Wadir 1'')';
    END
    ELSE IF @str = '14' OR @str = '54' OR @str = '26' OR @str = '19' 
    BEGIN
        -- Admin/Staff tertentu: lihat hampir semua status
        SET @viewBy = ' AND a.pdi_status IN (''Belum Disetujui Wadir 1'', ''Belum Disetujui Prodi'', ''Menunggu Upload SK'')';
    END
    ELSE IF @str = '27' OR @str = '23' OR @str = '28' 
    BEGIN
        -- Staff upload SK: lihat yang sudah disetujui dan menunggu upload
        SET @viewBy = ' AND a.pdi_status IN (''Disetujui'', ''Menunggu Upload SK'')';
    END
    ELSE 
    BEGIN
        -- Sekprodi: lihat data konsentrasi sendiri
        SET @viewBy = ' AND c.kon_sekprodi = ''' + @kryid + '''';
    END;

-- Build status filter
IF @status IS NOT NULL AND @status != ''
BEGIN
DECLARE @statusPattern VARCHAR(MAX) = ',' + @status + ',';
SET @statusFilter = ' AND CHARINDEX('','' + a.pdi_status + '','', ''' + @statusPattern + ''') > 0';
END

-- Set default sorting jika kosong
-- Priority: Draft & Revisi on top, then by last modified date (newest first)
IF @sort_by IS NULL OR @sort_by = ''
BEGIN
SET @sort_by = 'CASE WHEN a.pdi_status IN (''Draft'', ''Revisi'') THEN 0 ELSE 1 END, COALESCE(a.pdi_modif_date, a.pdi_created_date) DESC';
END

-- Build dynamic SQL dengan ROW_NUMBER pagination
-- CHANGED: Filter berdasarkan pro_id dan pro_nama instead of kon_id/kon_nama
SET @SQL = 'SELECT * FROM (
SELECT ROW_NUMBER() OVER (ORDER BY ' + @sort_by + ') AS rownum,
a.pdi_id,
b.mhs_id,
b.mhs_id + '' - '' + b.mhs_nama AS mhs_nama,
d.pro_singkatan + '' ('' + c.kon_singkatan + '')'' AS kon_nama,
CONVERT(VARCHAR(11), a.pdi_created_date, 106) AS pdi_created_date,
a.pdi_created_by,
a.srt_no,
a.pdi_status,
COUNT(*) OVER() AS [Count]
FROM sia_mspengundurandiri a
INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
INNER JOIN sia_mskonsentrasi c ON b.kon_id = c.kon_id
INNER JOIN sia_msprodi d ON c.pro_id = d.pro_id
WHERE (CASE 
WHEN ''' + @kon_id + ''' = '''' THEN 1 
WHEN d.pro_id = ''' + @kon_id + ''' THEN 1
WHEN d.pro_nama = ''' + @kon_id + ''' THEN 1
ELSE 0 END) = 1
AND (a.pdi_id LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
OR b.mhs_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
OR c.kon_nama LIKE ''%'' + ''' + @keyword + ''' + ''%'' 
OR a.srt_no LIKE ''%'' + ''' + @keyword + ''' + ''%'') '
+ @viewBy + @statusFilter + 
') res';

-- Tambahkan pagination jika parameter ada
IF @Page IS NOT NULL AND @PageSize IS NOT NULL
BEGIN
SET @SQL = @SQL + ' WHERE rownum BETWEEN ' + CAST((@Page - 1) * @PageSize + 1 AS VARCHAR) + ' AND ' + CAST((@Page * @PageSize) AS VARCHAR);
END

SET @SQL = @SQL + ';';

EXEC(@SQL);
END
GO
