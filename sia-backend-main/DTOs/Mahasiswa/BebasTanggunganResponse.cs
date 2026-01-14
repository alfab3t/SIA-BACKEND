namespace astratech_apps_backend.DTOs.Mahasiswa
{
    /// <summary>
    /// Response untuk check bebas tanggungan mahasiswa
    /// </summary>
    public class BebasTanggunganResponse
    {
        /// <summary>
        /// Status bebas tanggungan: "OK" atau "NOK"
        /// </summary>
        public string Status { get; set; } = "";
    }
}
