namespace ComedorSistema.Models.ViewModels
{
    public class PedidoListadoVm
    {
        public IEnumerable<PedidoComidum> Datos { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
