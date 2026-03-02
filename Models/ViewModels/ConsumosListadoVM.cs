namespace ComedorSistema.Models.ViewModels
{
    public class ConsumosListadoVM
    {
        public List<PedidoComidum> Datos { get; set; } = new();

        // ===================== PAGINACIÓN =====================
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }

        // ===================== FILTROS =====================
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}