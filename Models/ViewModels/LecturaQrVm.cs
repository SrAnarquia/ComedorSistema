namespace ComedorSistema.Models.ViewModels
{
    public class LecturaQrVm
    {
        public string CodigoQr { get; init; } = string.Empty;
        public DateTime FechaLectura { get; init; } = DateTime.Now;
        public string Usuario { get; init; } = string.Empty;

        public decimal? PrecioManual { get; set; }
        
        public string? Servicio { get; set; }

        public string? Descripcion { get; set; }

        public int? Cantidad { get; set; }

    }
}
