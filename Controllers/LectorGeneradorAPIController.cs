using ComedorSistema.Models;
using ComedorSistema.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace ComedorSistema.Controllers
{
    [ApiController]
    [Route("api/lector")]
    public class LectorGeneradorApiController : ControllerBase
    {
        #region builder
        private readonly ApplicationDbContext _context;
        private const string CLAVE_SECRETA = "$istEmas2026@4dmini5trado53e5c0M3dor";

        
        public LectorGeneradorApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        #region QRscanner

        [HttpPost("imagen")]
        public async Task<IActionResult> ProcesarImagen(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "No se envió ninguna imagen" });

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            byte[] imageBytes = ms.ToArray();

            // Procesar QR con OpenCV
            Mat mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);
            if (mat.Empty())
                return BadRequest(new { success = false, message = "No se pudo leer la imagen" });

            QRCodeDetector detector = new QRCodeDetector();
            string decodedText = detector.DetectAndDecode(mat, out Point2f[] points);

            if (string.IsNullOrEmpty(decodedText))
                return BadRequest(new { success = false, message = "No se detectó ningún QR" });

            // Enviar a Registrar
            var vm = new LecturaQrVm
            {
                CodigoQr = decodedText,
                FechaLectura = DateTime.Now,
                Usuario = "DesdeHTMLWebQR"
            };

            return Registrar(vm);
        }

       
        #endregion

        #region registrar   
        [HttpPost("registrar")]
            public IActionResult Registrar([FromBody] LecturaQrVm vm)
            {
                if (vm == null || string.IsNullOrWhiteSpace(vm.CodigoQr))
                    return BadRequest(new 
                    { mensaje = "QR vacío o inválido" });

                QrPayload data;

                try
                {
                    // Normaliza comillas simples del QR
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

            // Validar HMAC
            var firmaGenerada = GenerarFirmaHmac(
                data.empId,
                data.nombre,
                data.departamento
                );

            if (firmaGenerada != data.firma)
                return Unauthorized(new { mensaje = "Firma inválida. QR alterado." });


            /*
                var dataPlano = $"{data.empId}|{data.nombre}|{data.departamento}";
                var firmaGenerada = GenerarFirmaHmac(dataPlano);
            */


            if (firmaGenerada != data.firma)
                    return Unauthorized(new { mensaje = "Firma inválida. QR alterado." });

                // Validar doble escaneo (1 minuto)
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

                // Precio actual
                var precioActual = _context.Precios
                    .OrderByDescending(p => p.FechaActualizacion)
                    .Select(p => p.Precio1)
                    .FirstOrDefault();

                // Registrar pedido
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
                    mensaje = "Consumo registrado correctamente",
                    persona = data.nombre,
                    departamento = data.departamento,
                    precio = precioActual
                });
            }
            #endregion

        #region pruebas
        // ✅ Método de prueba GET
        [HttpGet("pruebas")]
        public IActionResult Pruebas()
        {
            var lista = Enumerable.Range(1, 10)
                                  .Select(n => new { numero = n })
                                  .ToList();

            return Ok(new
            {
                mensaje = "Prueba exitosa",
                datos = lista
            });

        }

        #endregion

        #region Historial
        [HttpGet("historial")]
        public IActionResult Historial(DateTime fecha)
        {
            var datos = _context.PedidoComida
                .Where(x => x.FechaCompra.HasValue &&
                            x.FechaCompra.Value.Date == fecha.Date)
                .OrderByDescending(x => x.FechaCompra)
                .Select(x => new
                {
                    nombre = x.Nombre,
                    precio=x.Precio,
                    hora = x.FechaCompra.Value.ToString("HH:mm:ss")
                })
                .ToList();

            return Ok(new
            {
                mensaje = "Registros del día",
                datos
            });
        }

        #endregion 

        #region GenerarFirmas
        private string GenerarFirmaHmac(string empId, string nombre, string departamento)
        {
            var data = $"{empId}|{nombre}|{departamento}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(CLAVE_SECRETA));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /*
        private string GenerarFirmaHmac(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(CLAVE_SECRETA));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        */
        #endregion
    }
}