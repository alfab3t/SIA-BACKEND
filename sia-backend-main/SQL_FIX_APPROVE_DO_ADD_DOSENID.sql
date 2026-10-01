-- Fix SP sia_approveDropOut - Tambahkan parameter @DosenId yang hilang

ALTER PROCEDURE [dbo].[sia_approveDropOut]
@dro_id VARCHAR(50),
@username VARCHAR(50)
AS
BEGIN
SET NOCOUNT ON;

DECLARE @str VARCHAR(MAX);

-- Ambil struktur organisasi user yang approve
SELECT @str = str_main_id FROM ess_mskaryawan WHERE kry_username = @username;

-- Hanya Wadir 1 (str_main_id = '2') yang bisa approve
IF @str = '2'
BEGIN
    DECLARE @srtno VARCHAR(50);
    DECLARE @srtno2 VARCHAR(50);
    DECLARE @jsu_id INT;
    DECLARE @kon_id INT;
    DECLARE @csu VARCHAR(20);

    -- Ambil data konsentrasi dan created_by dari dropout
    SELECT @csu = dro_created_by, @kon_id = kon_id 
    FROM sia_msdropout a
    INNER JOIN sia_msmahasiswa b ON a.mhs_id = b.mhs_id
    WHERE dro_id = RTRIM(@dro_id);

    -- Ambil jenis surat ID untuk SK Drop Out
    SELECT @jsu_id = jsu_id FROM sia_msjenissurat WHERE jsu_nama_surat = 'Surat Keputusan Drop Out';

    -- Generate nomor SK Drop Out
    -- FIX: Tambahkan parameter @DosenId dengan nilai NULL
    EXEC sia_createNoSurat 
        @JenisSuratId = @jsu_id, 
        @DosenId = NULL,              -- PARAMETER YANG DITAMBAHKAN
        @KonsentrasiId = @kon_id, 
        @CreatedBy = @username, 
        @NomorSurat = @srtno OUTPUT;

    -- Generate nomor Surat Keterangan (jsu_id = 5)
    EXEC sia_createNoSurat 
        @JenisSuratId = '5', 
        @DosenId = NULL,              -- PARAMETER YANG DITAMBAHKAN
        @KonsentrasiId = @kon_id, 
        @CreatedBy = @username, 
        @NomorSurat = @srtno2 OUTPUT;

    -- Update data dropout dengan nomor surat dan status
    UPDATE sia_msdropout
    SET
        srt_no = @srtno,
        dro_srt_ket_no = @srtno2,
        dro_status = 'Menunggu Upload SK',
        dro_appr_wadir1 = @username,
        dro_appr_wadir1_date = GETDATE(),
        dro_modif_by = @username,
        dro_modif_date = GETDATE()
    WHERE dro_id = @dro_id;

    -- Update status kuliah mahasiswa menjadi Drop Out
    UPDATE sia_msmahasiswa
    SET
        mhs_status_kuliah = 'Drop Out',
        mhs_modif_by = @username,
        mhs_modif_date = GETDATE()
    WHERE mhs_id = (SELECT mhs_id FROM sia_msdropout WHERE dro_id = @dro_id);

    -- Update status beasiswa (set tahun ajaran akhir dan semester akhir)
    WITH CTE AS (
        SELECT TOP 1 * 
        FROM sia_trpenerimabeasiswa
        WHERE mhs_id = (SELECT mhs_id FROM sia_msdropout WHERE dro_id = @dro_id)
        ORDER BY pba_id DESC
    )
    UPDATE CTE 
    SET 
        pba_tahun_ajaran_akhir = (
            SELECT TOP 1 kak_tahun_ajaran 
            FROM sia_mskalenderakademik 
            WHERE GETDATE() >= kak_tgl_from AND jke_id = 1 AND kak_status = 'Aktif' 
            ORDER BY kak_tgl_from DESC
        ),
        pba_semester_akhir = (
            SELECT TOP 1 kak_ganjil_genap 
            FROM sia_mskalenderakademik 
            WHERE GETDATE() >= kak_tgl_from AND jke_id = 1 AND kak_status = 'Aktif' 
            ORDER BY kak_tgl_from DESC
        );
END
END
