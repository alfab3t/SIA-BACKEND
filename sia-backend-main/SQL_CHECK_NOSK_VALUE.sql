-- Cek apakah NoSK ada di database untuk record yang baru diupload
-- Ganti pdi_id dengan ID yang sesuai

SELECT TOP 5
    pdi_id,
    mhs_id,
    pdi_status,
    srt_no,
    pdi_sk,
    pdi_skpb,
    pdi_created_date
FROM sia_mspengundurandiri
WHERE pdi_status IN ('Disetujui', 'Ditolak Prodi', 'Ditolak Wadir1')
ORDER BY pdi_created_date DESC;

-- Cek detail untuk satu record
-- EXEC sia_getDetailPengunduranDiri @pdi_id = 'PD-2026-0001';
