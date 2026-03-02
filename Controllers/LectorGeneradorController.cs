using ComedorSistema.Models;
using ComedorSistema.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ComedorSistema.Controllers
{
    public class LectorGeneradorController : Controller
    {
        #region Builder
        private readonly ApplicationDbContext _context;

        // CLAVE SECRETA HMAC (MISMA QUE PYTHON)
        private const string CLAVE_SECRETA = "$istEmas2026@4dmini5trado53e5c0M3dor";

        public LectorGeneradorController(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        #region IndexMasterList
        /* ============================================================
           VISTAS
        ============================================================ */

        // MASTER LIST (MIS PEDIDOS / CONSUMOS)
        public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1)
        {
            int pageSize = 10;

            var query = _context.PedidoComida.AsQueryable();

            // ===================== FILTROS =====================
            if (fechaInicio.HasValue)
            {
                query = query.Where(x => x.FechaCreacion >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                // Incluye TODO el día
                var finDia = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.FechaCreacion <= finDia);
            }

            // ===================== ORDEN =====================
            query = query.OrderByDescending(x => x.FechaCreacion);

            // ===================== PAGINACIÓN =====================
            int totalRegistros = query.Count();

            var datosPaginados = query
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new ConsumosListadoVM
            {
                Datos = datosPaginados,
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            return View(vm);
        }
        #endregion

        #region LecturaQRView
        // VISTA LECTOR QR
        public IActionResult Lectura()
        {
            return View();
        }
        #endregion

        #region RegistroLectura
        /* ============================================================
           REGISTRO DE LECTURA QR (CORE DEL SISTEMA)
        ============================================================ */
        [HttpPost]
        public IActionResult RegistrarLectura([FromBody] LecturaQrVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm?.CodigoQr))
                return BadRequest(new { mensaje = "QR vacío o inválido" });

            QrPayload data;

            try
            {
                // 🔧 NORMALIZA COMILLAS (PYTHON / JS MAL FORMATEADO)
                var json = vm.CodigoQr.Replace('\'', '"');

                data = JsonSerializer.Deserialize<QrPayload>(json);
            }
            catch
            {
                return BadRequest(new { mensaje = "Formato de QR incorrecto" });
            }

            if (data == null ||
                string.IsNullOrWhiteSpace(data.empId) ||
                string.IsNullOrWhiteSpace(data.nombre) ||
                string.IsNullOrWhiteSpace(data.departamento) ||
                string.IsNullOrWhiteSpace(data.firma))
            {
                return BadRequest(new { mensaje = "QR incompleto" });
            }

            if (!int.TryParse(data.empId, out int idPersona))
                return BadRequest(new { mensaje = "ID inválido" });

            //  VALIDAR HMAC
            var dataPlano = $"{data.empId}|{data.nombre}|{data.departamento}";
            var firmaGenerada = GenerarFirmaHmac(dataPlano);

            if (firmaGenerada != data.firma)
                return Unauthorized(new { mensaje = "Firma inválida. QR alterado." });

            //  VALIDAR DOBLE ESCANEO (2 MIN)
            var ultimoRegistro = _context.PedidoComida
                .Where(x => x.IdPersona == idPersona)
                .OrderByDescending(x => x.FechaCreacion)
                .FirstOrDefault();

            if (ultimoRegistro != null &&
                ultimoRegistro.FechaCreacion.HasValue &&
                (DateTime.Now - ultimoRegistro.FechaCreacion.Value).TotalMinutes < 1)
            {
                return BadRequest(new
                {
                    mensaje = $"⚠️ Ya registraste un consumo recientemente. Espera 1 minuto."
                });
            }

            //  PRECIO ACTUAL
            var precioActual = _context.Precios
                .OrderByDescending(p => p.FechaActualizacion)
                .Select(p => p.Precio1)
                .FirstOrDefault();

            //  REGISTRO
            var pedido = new PedidoComidum
            {
                IdPersona = idPersona,
                Nombre = data.nombre,
                Departamento = data.departamento,
                Precio = precioActual,
                Cantidad = 1,
                FechaCompra = DateTime.Now,
                FechaCreacion = DateTime.Now
            };

            _context.PedidoComida.Add(pedido);
            _context.SaveChanges();

            return Ok(new
            {
                mensaje = " Consumo registrado correctamente",
                persona = data.nombre,
                departamento = data.departamento,
                precio = precioActual
            });
        }
        #endregion

        #region PrecioCobrar
        //Se llama a la vista
        [HttpGet]
        public IActionResult PrecioCobrar() 
        {

            return View();
        }



        [HttpPost]
        public IActionResult PrecioCobrarUpdate(decimal? nuevoPrecio)
        {
            try
            {
                var precioActual = _context.Precios.FirstOrDefault();

                if (precioActual == null)
                {
                    // Primera vez (autocreación)
                    precioActual = new Precio
                    {
                        Precio1 = nuevoPrecio,
                        PrecioAnterior = null,
                        FechaActualizacion = DateTime.Now
                    };

                    _context.Precios.Add(precioActual);
                }
                else
                {
                    // Actualización normal
                    precioActual.PrecioAnterior = precioActual.Precio1;
                    precioActual.Precio1 = nuevoPrecio;
                    precioActual.FechaActualizacion = DateTime.Now;
                }

                _context.SaveChanges();

                return Ok(new
                {
                    mensaje = $"Nuevo precio establecido: {nuevoPrecio}"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    mensaje = "Algo salió mal, intenta nuevamente"
                });
            }
        }

        #endregion


        #region ExportExcel
        /* ============================================================
           EXPORTAR EXCEL
        ============================================================ */
        public IActionResult ExportarExcel(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.PedidoComida.AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(x => x.FechaCreacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(x => x.FechaCreacion <= fechaFin.Value.AddDays(1));

            var datos = query.ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.AddWorksheet("Consumos");

            ws.Cell(1, 1).Value = "ID Persona";
            ws.Cell(1, 2).Value = "Nombre";
            ws.Cell(1, 3).Value = "Departamento";
            ws.Cell(1, 4).Value = "Fecha";
            ws.Cell(1, 5).Value = "Precio";

            int row = 2;
            foreach (var item in datos)
            {
                ws.Cell(row, 1).Value = item.IdPersona;
                ws.Cell(row, 2).Value = item.Nombre;
                ws.Cell(row, 3).Value = item.Departamento;
                ws.Cell(row, 4).Value = item.FechaCreacion;
                ws.Cell(row, 5).Value = item.Precio;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Consumos_Comedor.xlsx"
            );
        }

        #endregion


        #region HMACHelper
        /* ============================================================
           HMAC
        ============================================================ */
        private string GenerarFirmaHmac(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(CLAVE_SECRETA));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            // HEX, NO BASE64
            return BitConverter.ToString(hash)
                .Replace("-", "")
                .ToLower();
        }
        #endregion



        #region EditarPrecioComida

        [HttpGet]
        public IActionResult GetPrecioActual()
        {
            var precio = _context.Precios
                .OrderByDescending(p => p.FechaActualizacion)
                .Select(p => p.Precio1)
                .FirstOrDefault();

            return Json(precio);
        }

        [HttpGet]
        public IActionResult EditarPrecioPartial()
        {
            var data = _context.Precios
                .OrderByDescending(p => p.FechaActualizacion)
                .Select(p => new EditarPrecioVM
                {
                    Precio = p.Precio1,
                    FechaActualizacion = p.FechaActualizacion
                })
                .FirstOrDefault();

            return PartialView("_EditarPrecio", data);
        }

        #endregion


    }
}