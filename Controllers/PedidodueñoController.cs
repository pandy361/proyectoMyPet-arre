using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Filters;
using proyecto_mejoradoMy_pet.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace proyecto_mejoradoMy_pet.Controllers
{
    [PreventPageReload]
    public class PedidodueñoController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PedidodueñoController(BdMypetv3Context context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: Pedidosdueño/MisPedidos
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> MisPedidos()
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedidos = await _context.TbPedidos
                .AsNoTracking()
                .Include(p => p.IdUsuarioPrestadorNavigation)
                .Include(p => p.IdDireccionNavigation)
                .Include(p => p.TbDetallePedidos)
                    .ThenInclude(d => d.IdServicioNavigation)
                .Include(p => p.TbPedidoMascota)
                    .ThenInclude(pm => pm.IdMascotaNavigation)
                .Where(p => p.IdUsuarioDueño == userId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.IsAuthenticated = true;
            ViewBag.UserType = HttpContext.Session.GetString("UserType");

            return View(pedidos);
        }

        // ✅ CORREGIDO: GET: Pedidosdueño/DetallePedido/{id}
        [HttpGet]
        public async Task<IActionResult> DetallePedido(int id)
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            try
            {
                // Obtener el pedido con todas sus relaciones
                var pedido = await _context.TbPedidos
                    .Include(p => p.IdUsuarioDueñoNavigation)
                    .Include(p => p.IdUsuarioPrestadorNavigation)
                    .Include(p => p.IdDireccionNavigation)
                    .Include(p => p.TbDetallePedidos)
                        .ThenInclude(d => d.IdServicioNavigation)
                    .Include(p => p.TbPedidoMascota)
                        .ThenInclude(pm => pm.IdMascotaNavigation)
                    .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuarioDueño == userId);

                if (pedido == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("MisPedidos");
                }

                // ✅ OBTENER EL PRESTADOR DIRECTAMENTE DE LA BD
                var prestador = await _context.TbPrestadores
                    .FirstOrDefaultAsync(p => p.IdUsuario == pedido.IdUsuarioPrestador);

                // Verificar si ya existe una reseña para este pedido
                var reseñaExistente = await _context.TbReseñas
                    .FirstOrDefaultAsync(r => r.IdPedido == id);

                // Crear el ViewModel
                var viewModel = new DetallePedidoViewModel
                {
                    IdPedido = pedido.IdPedido,
                    Estado = pedido.Estado,
                    FechaPedido = pedido.FechaPedido,
                    FechaServicio = pedido.FechaServicio,
                    HoraInicio = pedido.HoraServicioFrom,
                    HoraFin = pedido.HoraServicioTo,
                    Total = pedido.Total,

                    // Prestador
                    IdPrestador = prestador?.IdPrestador ?? 0,
                    NombrePrestador = $"{pedido.IdUsuarioPrestadorNavigation.PrimerNombre} {pedido.IdUsuarioPrestadorNavigation.PrimerApellido}",
                    TelefonoPrestador = pedido.IdUsuarioPrestadorNavigation.Telefono ?? "No disponible",
                    EmailPrestador = pedido.IdUsuarioPrestadorNavigation.Correo,
                    CalificacionPrestador = prestador?.CalificacionPromedio ?? 0,
                    
                    DescripcionPrestador = prestador?.Resumen,

                    // Dueño
                    NombreDueño = $"{pedido.IdUsuarioDueñoNavigation.PrimerNombre} {pedido.IdUsuarioDueñoNavigation.PrimerApellido}",
                    TelefonoDueño = pedido.IdUsuarioDueñoNavigation.Telefono ?? "No disponible",
                    EmailDueño = pedido.IdUsuarioDueñoNavigation.Correo,

                    // Dirección
                    DireccionCompleta = $"{pedido.IdDireccionNavigation.Direccion}, {pedido.IdDireccionNavigation.Ciudad}, {pedido.IdDireccionNavigation.Departamento}",

                    // Servicios
                    Servicios = pedido.TbDetallePedidos.Select(d => new ServicioDetalle
                    {
                        Nombre = d.IdServicioNavigation.Nombre,
                        Precio = d.IdServicioNavigation.Precio,
                        Cantidad = d.Cantidad ?? 1,
                        Subtotal = d.Subtotal
                    }).ToList(),

                    // Mascotas
                    Mascotas = pedido.TbPedidoMascota.Select(pm => new MascotaDetalle
                    {
                        Nombre = pm.IdMascotaNavigation.Nombre,
                        Tipo = pm.IdMascotaNavigation.Tipo,
                        Raza = pm.IdMascotaNavigation.Raza
                    }).ToList(),

                    // Reseña (si existe)
                    Reseña = reseñaExistente != null ? new ReseñaDetalle
                    {
                        IdReseña = reseñaExistente.IdReseña,
                        Calificacion = reseñaExistente.Calificacion ?? 0,
                        Comentario = reseñaExistente.Comentario,
                        Fecha = reseñaExistente.Fecha ?? DateTime.Now
                    } : null,

                    // Control de reseñas
                    PuedeDejarReseña = pedido.Estado == "Completado",
                    YaDejoReseña = reseñaExistente != null
                };

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.IsAuthenticated = true;
                ViewBag.UserType = HttpContext.Session.GetString("UserType");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los detalles: {ex.Message}";
                return RedirectToAction("MisPedidos");
            }
        }

        // ✅ AGREGAR ESTE MÉTODO AL PedidodueñoController existente

        // POST: Pedidosdueño/CancelarPedido
        [HttpPost]
        public async Task<IActionResult> CancelarPedido([FromBody] CancelarPedidoRequest request)
        {
            Console.WriteLine("===== INICIO CANCELACIÓN =====");
            Console.WriteLine($"ID Pedido recibido: {request?.IdPedido}");

            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                Console.WriteLine("❌ No autenticado");
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            Console.WriteLine($"UserId en sesión: {userIdString}");

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                Console.WriteLine("❌ Usuario no identificado");
                return Json(new { success = false, message = "Usuario no identificado" });
            }

            if (request == null || request.IdPedido <= 0)
            {
                Console.WriteLine("❌ ID de pedido inválido");
                return Json(new { success = false, message = "ID de pedido inválido" });
            }

            try
            {
                Console.WriteLine($"🔍 Buscando pedido #{request.IdPedido} para usuario #{userId}");

                var pedido = await _context.TbPedidos
                    .FirstOrDefaultAsync(p => p.IdPedido == request.IdPedido && p.IdUsuarioDueño == userId);

                if (pedido == null)
                {
                    Console.WriteLine($"❌ Pedido no encontrado. IdPedido={request.IdPedido}, UserId={userId}");

                    // Debug: verificar si existe el pedido sin filtro de usuario
                    var pedidoExiste = await _context.TbPedidos.AnyAsync(p => p.IdPedido == request.IdPedido);
                    Console.WriteLine($"   ¿Existe el pedido sin filtro?: {pedidoExiste}");

                    return Json(new { success = false, message = "Pedido no encontrado o no autorizado" });
                }

                Console.WriteLine($"✅ Pedido encontrado. Estado actual: {pedido.Estado}");

                if (pedido.Estado == "Completado")
                {
                    return Json(new { success = false, message = "No puedes cancelar un pedido completado" });
                }

                if (pedido.Estado == "Cancelado")
                {
                    return Json(new { success = false, message = "Este pedido ya está cancelado" });
                }

                // Validación de 24 horas
                DateTime fechaHoraServicio = new DateTime(
                    pedido.FechaServicio.Year,
                    pedido.FechaServicio.Month,
                    pedido.FechaServicio.Day,
                    pedido.HoraServicioFrom.Hour,
                    pedido.HoraServicioFrom.Minute,
                    0
                );
                DateTime ahora = DateTime.Now;
                TimeSpan diferencia = fechaHoraServicio - ahora;
                double horasRestantes = diferencia.TotalHours;

                Console.WriteLine($"🕐 Validación: Servicio {fechaHoraServicio:dd/MM/yyyy HH:mm}, Ahora {ahora:dd/MM/yyyy HH:mm}, Horas: {horasRestantes:F2}");

                if (horasRestantes < 24)
                {
                    return Json(new { success = false, message = $"Solo puedes cancelar con al menos 24 horas de anticipación. Te quedan {horasRestantes:F1} horas." });
                }

                pedido.Estado = "Cancelado";
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Pedido #{request.IdPedido} cancelado exitosamente");
                Console.WriteLine("===== FIN CANCELACIÓN =====");

                return Json(new { success = true, message = "Pedido cancelado exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error al cancelar el pedido. Intenta nuevamente." });
            }
        }

        // ✅ AGREGAR ESTA CLASE AL FINAL DEL CONTROLADOR (fuera de los métodos)
       

        // ✅ CORREGIDO: POST: Pedidosdueño/CrearReseña
        [HttpPost]
        public async Task<IActionResult> CrearReseña([FromBody] CrearReseñaRequest request)
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "Usuario no identificado" });
            }

            // Validaciones
            if (request.Calificacion < 1 || request.Calificacion > 5)
            {
                return Json(new { success = false, message = "La calificación debe estar entre 1 y 5" });
            }

            try
            {
                // Verificar que el pedido existe y pertenece al usuario
                var pedido = await _context.TbPedidos
                    .Include(p => p.IdUsuarioPrestadorNavigation)
                    .FirstOrDefaultAsync(p => p.IdPedido == request.IdPedido && p.IdUsuarioDueño == userId);

                if (pedido == null)
                {
                    return Json(new { success = false, message = "Pedido no encontrado" });
                }

                // VERIFICAR QUE EL PEDIDO ESTÉ COMPLETADO
                if (pedido.Estado != "Completado")
                {
                    return Json(new { success = false, message = "Solo puedes dejar reseñas en pedidos completados" });
                }

                // Verificar si ya existe una reseña
                var reseñaExistente = await _context.TbReseñas
                    .FirstOrDefaultAsync(r => r.IdPedido == request.IdPedido);

                if (reseñaExistente != null)
                {
                    return Json(new { success = false, message = "Ya has dejado una reseña para este pedido" });
                }

                // Crear la reseña
                var nuevaReseña = new TbReseña
                {
                    IdPedido = request.IdPedido,
                    IdUsuario = userId,
                    Calificacion = request.Calificacion,
                    Comentario = request.Comentario,
                    Fecha = DateTime.Now
                };

                _context.TbReseñas.Add(nuevaReseña);
                await _context.SaveChangesAsync();

                // ✅ OBTENER Y ACTUALIZAR CALIFICACIÓN DEL PRESTADOR
                var prestador = await _context.TbPrestadores
                    .FirstOrDefaultAsync(p => p.IdUsuario == pedido.IdUsuarioPrestador);

                if (prestador != null)
                {
                    await ActualizarCalificacionPrestador(prestador.IdPrestador);
                }

                return Json(new { success = true, message = "¡Reseña enviada exitosamente!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al crear reseña: {ex.Message}");
                return Json(new { success = false, message = "Error al enviar la reseña: " + ex.Message });
            }
        }

        // ✅ MÉTODO PRIVADO: Actualizar calificación del prestador
        private async Task ActualizarCalificacionPrestador(int idPrestador)
        {
            var prestador = await _context.TbPrestadores
                .FirstOrDefaultAsync(p => p.IdPrestador == idPrestador);

            if (prestador != null)
            {
                // Obtener todas las reseñas del prestador
                var reseñas = await _context.TbReseñas
                    .Include(r => r.IdPedidoNavigation)
                    .Where(r => r.IdPedidoNavigation.IdUsuarioPrestador == prestador.IdUsuario)
                    .ToListAsync();

                if (reseñas.Any())
                {
                    var calificacionPromedio = reseñas
                        .Where(r => r.Calificacion.HasValue)
                        .Average(r => (decimal)r.Calificacion.Value);

                    prestador.CalificacionPromedio = calificacionPromedio;
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"✅ Calificación actualizada: {calificacionPromedio:F2}");
                }
            }
        }

        // GET: Pedidosdueño/VerFactura/{id}
        public async Task<IActionResult> VerFactura(int id)
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedido = await _context.TbPedidos
                .AsNoTracking()
                .Include(p => p.IdUsuarioPrestadorNavigation)
                .Include(p => p.IdUsuarioDueñoNavigation)
                .Include(p => p.IdDireccionNavigation)
                .Include(p => p.TbDetallePedidos)
                    .ThenInclude(d => d.IdServicioNavigation)
                .Include(p => p.TbPedidoMascota)
                    .ThenInclude(pm => pm.IdMascotaNavigation)
                .Include(p => p.TbPagos)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuarioDueño == userId);

            if (pedido == null)
            {
                return NotFound();
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.IsAuthenticated = true;
            ViewBag.UserType = HttpContext.Session.GetString("UserType");

            return View(pedido);
        }

        // GET: Pedidosdueño/GenerarFacturaPDF/{id}
        public async Task<IActionResult> GenerarFacturaPDF(int id)
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            var pedido = await _context.TbPedidos
                .AsNoTracking()
                .Include(p => p.IdUsuarioPrestadorNavigation)
                .Include(p => p.IdUsuarioDueñoNavigation)
                .Include(p => p.IdDireccionNavigation)
                .Include(p => p.TbDetallePedidos)
                    .ThenInclude(d => d.IdServicioNavigation)
                .Include(p => p.TbPedidoMascota)
                    .ThenInclude(pm => pm.IdMascotaNavigation)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuarioDueño == userId);

            if (pedido == null)
            {
                return NotFound();
            }

            var pdfBytes = GenerarPDFFactura(pedido);
            return File(pdfBytes, "application/pdf", $"Factura_Pedido_{pedido.IdPedido}.pdf");
        }

        private byte[] GenerarPDFFactura(TbPedido pedido)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("🐾 MY PET")
                                    .FontSize(28)
                                    .Bold()
                                    .FontColor("#667eea");

                                column.Item().Text("Servicios para Mascotas")
                                    .FontSize(12)
                                    .FontColor(Colors.Grey.Darken2);
                            });

                            row.RelativeItem().Column(column =>
                            {
                                column.Item().AlignRight().Text("FACTURA")
                                    .FontSize(24)
                                    .Bold();

                                column.Item().AlignRight().Text($"# {pedido.IdPedido}")
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Darken1);

                                column.Item().AlignRight().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                                    .FontSize(10);
                            });
                        });

                    page.Content()
                        .PaddingVertical(30)
                        .Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("CLIENTE")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor("#667eea");

                                    col.Item().PaddingTop(5).Text($"{pedido.IdUsuarioDueñoNavigation.PrimerNombre} {pedido.IdUsuarioDueñoNavigation.PrimerApellido}");
                                    col.Item().Text($"Tel: {pedido.IdUsuarioDueñoNavigation.Telefono}");
                                    col.Item().Text($"Email: {pedido.IdUsuarioDueñoNavigation.Correo}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("PRESTADOR")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor("#667eea");

                                    col.Item().PaddingTop(5).Text($"{pedido.IdUsuarioPrestadorNavigation.PrimerNombre} {pedido.IdUsuarioPrestadorNavigation.PrimerApellido}");
                                    col.Item().Text($"Tel: {pedido.IdUsuarioPrestadorNavigation.Telefono}");
                                    col.Item().Text($"Email: {pedido.IdUsuarioPrestadorNavigation.Correo}");
                                });
                            });

                            column.Item().PaddingTop(30).Column(col =>
                            {
                                col.Item().Text("DETALLES DEL SERVICIO")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor("#667eea");

                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Text("Fecha del Servicio:");
                                    row.RelativeItem().Text(pedido.FechaServicio.ToString("dd/MM/yyyy")).Bold();
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Hora:");
                                    row.RelativeItem().Text($"{pedido.HoraServicioFrom:HH\\:mm} - {pedido.HoraServicioTo:HH\\:mm}").Bold();
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Dirección:");
                                    row.RelativeItem().Text(pedido.IdDireccionNavigation.Direccion).Bold();
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Estado:");
                                    row.RelativeItem().Text(pedido.Estado).Bold().FontColor("#f59e0b");
                                });
                            });

                            if (pedido.TbPedidoMascota.Any())
                            {
                                column.Item().PaddingTop(20).Column(col =>
                                {
                                    col.Item().Text("MASCOTAS")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor("#667eea");

                                    col.Item().PaddingTop(10).Text(string.Join(", ", pedido.TbPedidoMascota.Select(pm => pm.IdMascotaNavigation.Nombre)));
                                });
                            }

                            column.Item().PaddingTop(30).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#667eea").Padding(10).Text("SERVICIO").Bold().FontColor(Colors.White);
                                    header.Cell().Background("#667eea").Padding(10).Text("CANTIDAD").Bold().FontColor(Colors.White);
                                    header.Cell().Background("#667eea").Padding(10).Text("SUBTOTAL").Bold().FontColor(Colors.White);
                                });

                                foreach (var detalle in pedido.TbDetallePedidos)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text(detalle.IdServicioNavigation.Nombre);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text((detalle.Cantidad ?? 1).ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text($"${detalle.Subtotal:N0}");
                                }
                            });

                            column.Item().PaddingTop(20).AlignRight().Column(col =>
                            {
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL:");
                                    row.ConstantItem(150).Text($"${pedido.Total:N0}")
                                        .FontSize(20)
                                        .Bold()
                                        .FontColor("#667eea");
                                });
                            });

                            column.Item().PaddingTop(40).Column(col =>
                            {
                                col.Item().Text("NOTAS")
                                    .FontSize(12)
                                    .Bold();

                                col.Item().PaddingTop(5).Text("Gracias por confiar en nuestros servicios. Para cualquier consulta, no dude en contactarnos.")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1))
                        .Text(text =>
                        {
                            text.Span("Generado el: ");
                            text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").Bold();
                        });
                });
            });

            return document.GeneratePdf();
        }
        public class CancelarPedidoRequest
        {
            public int IdPedido { get; set; }
        }
    }
}