-- ============================================
-- Test Script: sia_getDataPendingDO (Updated Version)
-- ============================================

USE [ERP_PolmanAstra_NDA]
GO

-- ============================================
-- 1. Test Basic - Tanpa Pagination (Semua Data)
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',           -- Username yang login
    @keyword = '',                 -- Keyword pencarian (kosong = semua)
    @sort_by = '',                 -- Kosong = pakai default sorting (Draft/Revisi on top + tanggal terbaru)
    @kon_id = '',                  -- Filter konsentrasi (kosong = semua)
    @role_id = '',                 -- Role ID
    @display_name = '',            -- Display name
    @status = '',                  -- Filter status (kosong = semua)
    @Page = NULL,                  -- NULL = tanpa pagination
    @PageSize = NULL;              -- NULL = tanpa pagination
GO

-- ============================================
-- 2. Test Dengan Pagination - Page 1, 10 records per page
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',                 -- Default sorting
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,                     -- Halaman 1
    @PageSize = 10;                -- 10 data per halaman
GO

-- ============================================
-- 3. Test Dengan Pagination - Page 2
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 2,                     -- Halaman 2
    @PageSize = 10;
GO

-- ============================================
-- 4. Test Dengan Filter Status (Multiple Status)
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = '',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = 'Draft,Revisi',      -- Filter hanya Draft dan Revisi
    @Page = 1,
    @PageSize = 10;
GO

-- ============================================
-- 5. Test Dengan Keyword Search
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = 'John',             -- Cari nama mahasiswa yang mengandung 'John'
    @sort_by = '',
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;
GO

-- ============================================
-- 6. Test Custom Sorting (Override Default)
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = '',
    @sort_by = 'mhs_nama ASC',     -- Sort by nama mahasiswa A-Z
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = '',
    @Page = 1,
    @PageSize = 10;
GO

-- ============================================
-- 7. Test Kombinasi: Filter Status + Keyword + Pagination
-- ============================================
EXEC sia_getDataPendingDO
    @username = 'admin',
    @keyword = 'TI',               -- Cari yang ada kata 'TI'
    @sort_by = '',                 -- Default sorting
    @kon_id = '',
    @role_id = '',
    @display_name = '',
    @status = 'Draft,Revisi,Belum Disetujui Wadir 1',  -- Multiple status
    @Page = 1,
    @PageSize = 5;                 -- 5 data per halaman
GO

-- ============================================
-- CATATAN PENTING:
-- ============================================
-- 1. Kolom 'Count' di result akan menunjukkan TOTAL records (sebelum pagination)
-- 2. Kolom 'rownum' menunjukkan nomor urut row
-- 3. Default sorting: Draft & Revisi muncul paling atas, lalu diurutkan tanggal terbaru
-- 4. @sort_by bisa diisi custom, contoh:
--    - 'dro_created_date DESC'
--    - 'mhs_nama ASC'
--    - 'dro_status ASC, dro_created_date DESC'
-- 5. @status bisa multiple, pisahkan dengan koma: 'Draft,Revisi,Disetujui'
