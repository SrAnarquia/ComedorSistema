using ComedorSistema.Models;
using ComedorSistema.Models.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ================== USINGS COMPLETOS ==================
using System;
using System.IO;
using System.Text.Json;

// Para ImageSharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// ZXing básico
using ZXing;

// Alias para ZXing.ImageSharp y evitar ambigüedad
using ZXingImageSharp = ZXing.ImageSharp;


using ComedorSistema.Models;
using ComedorSistema.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        // Usings necesarios (añádelos al inicio del archivo)

        // Método

        [HttpPost("imagen")]
        public IActionResult ProcesarImagen([FromBody] JsonElement body)
        {
            // 1️⃣ Validar input
            if (!body.TryGetProperty("imagenQr", out var qrElement))
                return BadRequest("No se envió imagenQr");

            var base64Image = qrElement.GetString();
            if (string.IsNullOrWhiteSpace(base64Image))
                return BadRequest("Imagen vacía");

            try
            {
                // 2️⃣ Convertir Base64 a ImageSharp
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                using var ms = new MemoryStream(imageBytes);
                using Image<Rgba32> image = Image.Load<Rgba32>(ms);

                // 3️⃣ Crear lector ZXing (ImageSharp) y decodificar
                var reader = new ZXingImageSharp.BarcodeReader<Rgba32>();
                ZXing.Result result = reader.Decode(image);

                if (result == null)
                    return BadRequest("No se pudo leer QR de la imagen");

                // 4️⃣ Obtener texto del QR
                string codigoQr = result.Text;

                // 5️⃣ Crear tu ViewModel y reutilizar el método Registrar
                var vm = new LecturaQrVm
                {
                    CodigoQr = codigoQr,
                    FechaLectura = DateTime.Now,
                    Usuario = "DesdeMAUIWebQR"
                };

                // 6️⃣ Llamar a tu método de registro
                return Registrar(vm);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error procesando QR: " + ex.Message });
            }
        }




        /*
        [HttpPost("imagen")]
        public IActionResult ProcesarImagen([FromBody] JsonElement body)
        {
            if (!body.TryGetProperty("imagenQr", out var qrElement))
                return BadRequest(new { mensaje = "No se envió imagenQr" });

            string codigoQr = qrElement.GetString();

            if (string.IsNullOrWhiteSpace(codigoQr))
                return BadRequest(new { mensaje = "QR vacío" });

            // 🔹 Reutilizamos el método de registro existente internamente
            var vm = new LecturaQrVm
            {
                CodigoQr = codigoQr,
                FechaLectura = DateTime.Now,
                Usuario = "DesdeImagenAPI"
            };

            // Llamamos al método de registrar directamente
            return Registrar(vm);
        }
        */

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
            var dataPlano = $"{data.empId}|{data.nombre}|{data.departamento}";
            var firmaGenerada = GenerarFirmaHmac(dataPlano);

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

        #region GenerarFirmas
        private string GenerarFirmaHmac(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(CLAVE_SECRETA));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        #endregion
    }
}