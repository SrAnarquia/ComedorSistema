using System;
using System.Collections.Generic;

namespace ComedorSistema.Models;

public partial class Precio
{
    public int Id { get; set; }

    public decimal? Precio1 { get; set; }

    public decimal? PrecioAnterior { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}
