using System;
using System.Collections.Generic;

namespace ComedorSistema.Models;

public partial class PedidoComidum
{
    public int Id { get; set; }

    public int? IdPersona { get; set; }

    public string? Nombre { get; set; }

    public int? IdDepartamento { get; set; }

    public string? Departamento { get; set; }

    public decimal? Precio { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? FechaCompra { get; set; }

    public DateTime? FechaCreacion { get; set; }
}
