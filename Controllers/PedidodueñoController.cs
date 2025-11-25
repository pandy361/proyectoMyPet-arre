using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PedidodueñoController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PedidodueñoController(BdMypetv3Context context)
        {
            _context = context;
            // Configuración de licencia para QuestPDF (Community)
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
                .Where(p => p.IdUsuarioDueño == userId &&
                           (p.Estado == "Pendiente" || p.Estado == "En Progreso"))
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.IsAuthenticated = true;
            ViewBag.UserType = HttpContext.Session.GetString("UserType");

            return View(pedidos);
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

            // Obtener el pedido completo
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

            // Generar el PDF
            var pdfBytes = GenerarPDFFactura(pedido);

            // Retornar el PDF
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
                            // Información del Cliente y Prestador
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

                            // Detalles del Servicio
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

                            // Mascotas
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

                            // Tabla de Servicios
                            column.Item().PaddingTop(30).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    header.Cell().Background("#667eea").Padding(10).Text("SERVICIO").Bold().FontColor(Colors.White);
                                    header.Cell().Background("#667eea").Padding(10).Text("CANTIDAD").Bold().FontColor(Colors.White);
                                    header.Cell().Background("#667eea").Padding(10).Text("SUBTOTAL").Bold().FontColor(Colors.White);
                                });

                                // Contenido
                                foreach (var detalle in pedido.TbDetallePedidos)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text(detalle.IdServicioNavigation.Nombre);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text((detalle.Cantidad ?? 1).ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text($"${detalle.Subtotal:N0}");
                                }
                            });

                            // Total
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

                            // Notas
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
     .AlignCenter() // Esto te devuelve un IContainer
     .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1)) // <-- Usa .TextStyle() para aplicar estilos a Contenedores
     .Text(text => // Ahora se define el contenido del texto
     {
         text.Span("Generado el: ");
         text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").Bold();
     });
                });
            });

            return document.GeneratePdf();
        }
    }
}