using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PedidosPrestadorController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PedidosPrestadorController(BdMypetv3Context context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IActionResult> Index()
        {
            // Verificar autenticación
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true" ||
                string.IsNullOrEmpty(userType) || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idPrestador))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            // Obtener el nombre del prestador
            var nombrePrestador = HttpContext.Session.GetString("UserName");
            ViewBag.NombrePrestador = nombrePrestador;

            // Obtener todos los pedidos del prestador
            var pedidos = await _context.TbPedidos
                .Include(p => p.IdUsuarioDueñoNavigation)
                .Include(p => p.IdDireccionNavigation)
                .Include(p => p.TbPedidoMascota)
                    .ThenInclude(pm => pm.IdMascotaNavigation)
                .Include(p => p.TbDetallePedidos)
                    .ThenInclude(dp => dp.IdServicioNavigation)
                .Where(p => p.IdUsuarioPrestador == idPrestador)
                .OrderByDescending(p => p.FechaPedido)
                .ThenByDescending(p => p.IdPedido)
                .ToListAsync();

            return View(pedidos);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int idPedido, string nuevoEstado)
        {
            // Verificar autenticación
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true" ||
                string.IsNullOrEmpty(userType) || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "No autenticado" });
            }

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idPrestador))
            {
                return Json(new { success = false, message = "Usuario no identificado" });
            }

            // Buscar el pedido y verificar que pertenezca al prestador
            var pedido = await _context.TbPedidos
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido && p.IdUsuarioPrestador == idPrestador);

            if (pedido == null)
            {
                return Json(new { success = false, message = "Pedido no encontrado o no autorizado" });
            }

            // Validar si está cancelado
            if (pedido.Estado == "Cancelado")
            {
                return Json(new { success = false, message = "Este pedido fue cancelado por el cliente" });
            }

            // Validar transiciones de estado válidas
            bool transicionValida = false;
            switch (pedido.Estado?.ToLower())
            {
                case "pendiente":
                case "pagado (pendiente)":
                    transicionValida = nuevoEstado == "Aceptado" || nuevoEstado == "Rechazado";
                    break;
                case "aceptado":
                    transicionValida = nuevoEstado == "En Proceso" || nuevoEstado == "Completado";
                    break;
                case "en proceso":
                    transicionValida = nuevoEstado == "Completado";
                    break;
                default:
                    transicionValida = false;
                    break;
            }

            if (!transicionValida)
            {
                return Json(new { success = false, message = "Transición de estado no válida" });
            }

            try
            {
                pedido.Estado = nuevoEstado;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Detalles(int id)
        {
            // Verificar autenticación
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true" ||
                string.IsNullOrEmpty(userType) || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idPrestador))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            // Obtener el pedido con todos sus detalles
            var pedido = await _context.TbPedidos
                .Include(p => p.IdUsuarioDueñoNavigation)
                .Include(p => p.IdUsuarioPrestadorNavigation)
                .Include(p => p.IdDireccionNavigation)
                .Include(p => p.TbPedidoMascota)
                    .ThenInclude(pm => pm.IdMascotaNavigation)
                .Include(p => p.TbDetallePedidos)
                    .ThenInclude(dp => dp.IdServicioNavigation)
                .Include(p => p.TbPagos)
                .Include(p => p.TbReseñas)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuarioPrestador == idPrestador);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido no encontrado";
                return RedirectToAction("Index");
            }

            ViewBag.NombrePrestador = HttpContext.Session.GetString("UserName");
            return View(pedido);
        }

        // Generar factura PDF
        public async Task<IActionResult> GenerarFacturaPDF(int id)
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true" ||
                string.IsNullOrEmpty(userType) || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idPrestador))
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
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuarioPrestador == idPrestador);

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

                                    col.Item().PaddingTop(5).Text($"{pedido.IdUsuarioDueñoNavigation?.PrimerNombre} {pedido.IdUsuarioDueñoNavigation?.PrimerApellido}");
                                    col.Item().Text($"Tel: {pedido.IdUsuarioDueñoNavigation?.Telefono ?? "N/A"}");
                                    col.Item().Text($"Email: {pedido.IdUsuarioDueñoNavigation?.Correo ?? "N/A"}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("PRESTADOR")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor("#667eea");

                                    col.Item().PaddingTop(5).Text($"{pedido.IdUsuarioPrestadorNavigation?.PrimerNombre} {pedido.IdUsuarioPrestadorNavigation?.PrimerApellido}");
                                    col.Item().Text($"Tel: {pedido.IdUsuarioPrestadorNavigation?.Telefono ?? "N/A"}");
                                    col.Item().Text($"Email: {pedido.IdUsuarioPrestadorNavigation?.Correo ?? "N/A"}");
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
                                    row.RelativeItem().Text($"{pedido.HoraServicioFrom:hh\\:mm} - {pedido.HoraServicioTo:hh\\:mm}").Bold();
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Dirección:");
                                    row.RelativeItem().Text(pedido.IdDireccionNavigation?.Direccion ?? "N/A").Bold();
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Estado:");
                                    row.RelativeItem().Text(pedido.Estado ?? "Pendiente").Bold().FontColor("#f59e0b");
                                });
                            });

                            if (pedido.TbPedidoMascota != null && pedido.TbPedidoMascota.Any())
                            {
                                column.Item().PaddingTop(20).Column(col =>
                                {
                                    col.Item().Text("MASCOTAS")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor("#667eea");

                                    col.Item().PaddingTop(10).Text(string.Join(", ", pedido.TbPedidoMascota.Select(pm => pm.IdMascotaNavigation?.Nombre ?? "N/A")));
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

                                if (pedido.TbDetallePedidos != null)
                                {
                                    foreach (var detalle in pedido.TbDetallePedidos)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text(detalle.IdServicioNavigation?.Nombre ?? "N/A");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text((detalle.Cantidad ?? 1).ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text($"${detalle.Subtotal:N0}");
                                    }
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

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Autenticacion");
        }
    }
}