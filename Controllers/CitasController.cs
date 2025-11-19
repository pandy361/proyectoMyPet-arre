using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;
using System;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class CitasController : Controller
    {
        private readonly BdMypetv3Context _context;

        public CitasController(BdMypetv3Context context)
        {
            _context = context;
        }

        // Verificar autenticación
        private bool EstaAutenticado()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        private int? ObtenerIdUsuario()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out int userId))
                return userId;
            return null;
        }

        // VISTA DE AGENDAMIENTO
        [HttpGet]
        public async Task<IActionResult> Agendar(int idPrestador)
        {
            if (!EstaAutenticado())
            {
                TempData["Error"] = "Debes iniciar sesión para agendar un servicio";
                return RedirectToAction("Login", "Autenticacion");
            }

            var userId = ObtenerIdUsuario();
            if (userId == null)
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            try
            {
                // Obtener información del prestador
                var prestador = await _context.TbPrestadores
                    .Include(p => p.IdUsuarioNavigation)
                    .Include(p => p.TbServicios)
                    .Include(p => p.TbDisponibilidads)
                    .FirstOrDefaultAsync(p => p.IdPrestador == idPrestador);

                if (prestador == null)
                {
                    TempData["Error"] = "Prestador no encontrado";
                    return RedirectToAction("Index", "Prestadores");
                }

                // Obtener mascotas del usuario
                var mascotas = await _context.TbMascotas
                    .Where(m => m.IdUsuario == userId.Value)
                    .ToListAsync();

                // Obtener direcciones del usuario
                var direcciones = await _context.TbDirecciones
                    .Where(d => d.IdUsuario == userId.Value)
                    .ToListAsync();

                var viewModel = new AgendarPedidoViewModel
                {
                    IdPrestador = prestador.IdPrestador,
                    NombrePrestador = $"{prestador.IdUsuarioNavigation.PrimerNombre} {prestador.IdUsuarioNavigation.PrimerApellido}",
                    CalificacionPrestador = prestador.CalificacionPromedio ?? 0,
                    ServiciosDisponibles = prestador.TbServicios.Select(s => new ServicioSeleccionViewModel
                    {
                        IdServicio = s.IdServicio,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        Precio = s.Precio,
                        Seleccionado = false
                    }).ToList(),
                    Disponibilidad = prestador.TbDisponibilidads.Select(d => new DisponibilidadViewModel
                    {
                        DiaSemana = d.DiaSemana,
                        HoraInicio = d.HoraInicio.ToString(@"hh\:mm"),
                        HoraFin = d.HoraFin.ToString(@"hh\:mm"),
                    }).ToList(),
                    MascotasUsuario = mascotas.Select(m => new MascotaSeleccionViewModel
                    {
                        IdMascota = m.IdMascota,
                        Nombre = m.Nombre,
                        Tipo = m.Tipo,
                        Raza = m.Raza,
                        Seleccionada = false
                    }).ToList(),
                    DireccionesUsuario = direcciones.Select(d => new DireccionSeleccionViewModel
                    {
                        IdDireccion = d.IdDireccion,
                        DireccionCompleta = $"{d.Direccion}, {d.Ciudad}, {d.Departamento}",
                        EsPredeterminada = d.EsPredeterminada ?? false
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction("Index", "Prestadores");
            }
        }

        // PROCESAR AGENDAMIENTO
        [HttpPost]
        public async Task<IActionResult> ProcesarAgendamiento([FromBody] ProcesarPedidoRequest request)
        {
            if (!EstaAutenticado())
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            var userId = ObtenerIdUsuario();
            if (userId == null)
            {
                return Json(new { success = false, message = "Usuario no identificado" });
            }

            // Validaciones
            if (request.IdPrestador <= 0)
            {
                return Json(new { success = false, message = "Prestador no válido" });
            }

            if (request.ServiciosSeleccionados == null || !request.ServiciosSeleccionados.Any())
            {
                return Json(new { success = false, message = "Debes seleccionar al menos un servicio" });
            }

            if (request.MascotasSeleccionadas == null || !request.MascotasSeleccionadas.Any())
            {
                return Json(new { success = false, message = "Debes seleccionar al menos una mascota" });
            }

            if (request.IdDireccion <= 0)
            {
                return Json(new { success = false, message = "Debes seleccionar una dirección" });
            }

            // Validar que se recibió el ID de transacción de PayPal
            if (string.IsNullOrEmpty(request.PaypalOrderId))
            {
                return Json(new { success = false, message = "No se recibió confirmación de PayPal" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener el usuario prestador
                var prestador = await _context.TbPrestadores
                    .FirstOrDefaultAsync(p => p.IdPrestador == request.IdPrestador);

                if (prestador == null)
                {
                    return Json(new { success = false, message = "Prestador no encontrado" });
                }

                // Calcular el total
                var servicios = await _context.TbServicios
                    .Where(s => request.ServiciosSeleccionados.Contains(s.IdServicio))
                    .ToListAsync();

                int total = servicios.Sum(s => s.Precio);

                // Parsear fecha y horas
                DateOnly fechaServicio = DateOnly.Parse(request.FechaServicio);
                TimeOnly horaInicio = TimeOnly.Parse(request.HoraInicio);
                TimeOnly horaFin = TimeOnly.Parse(request.HoraFin);

                // Crear el pedido
                var pedido = new TbPedido
                {
                    IdUsuarioDueño = userId.Value,
                    IdUsuarioPrestador = prestador.IdUsuario,
                    IdDireccion = request.IdDireccion,
                    FechaPedido = DateOnly.FromDateTime(DateTime.Now),
                    FechaServicio = fechaServicio,
                    HoraServicioFrom = horaInicio,
                    HoraServicioTo = horaFin,
                    Total = total,
                    Estado = "Pagado" // ✅ Ya está pagado por PayPal
                };

                _context.TbPedidos.Add(pedido);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Pedido creado con ID: {pedido.IdPedido}");

                // Registrar el pago de PayPal
                var pago = new TbPago
                {
                    IdPedido = pedido.IdPedido,
                    Monto = total,
                    FechaPago = DateTime.Now,
                    Metodo = "PayPal",
                    Estado = "Completado",
                    IdTransaccionPaypal = request.PaypalOrderId
                };
                _context.TbPagos.Add(pago);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Pago registrado con transacción PayPal: {request.PaypalOrderId}");

                // Agregar mascotas al pedido
                foreach (var idMascota in request.MascotasSeleccionadas)
                {
                    var pedidoMascota = new TbPedidoMascota
                    {
                        IdPedido = pedido.IdPedido,
                        IdMascota = idMascota
                    };
                    _context.TbPedidoMascotas.Add(pedidoMascota);
                }

                // Agregar detalle de servicios
                foreach (var idServicio in request.ServiciosSeleccionados)
                {
                    var servicio = servicios.First(s => s.IdServicio == idServicio);
                    var detalle = new TbDetallePedido
                    {
                        IdPedido = pedido.IdPedido,
                        IdServicio = idServicio,
                        Cantidad = 1,
                        Subtotal = servicio.Precio
                    };
                    _context.TbDetallePedidos.Add(detalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                System.Diagnostics.Debug.WriteLine($"🎉 Pedido {pedido.IdPedido} completado exitosamente con pago PayPal");

                return Json(new
                {
                    success = true,
                    message = "Pedido agendado y pagado exitosamente",
                    idPedido = pedido.IdPedido
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al procesar el pedido: {ex.Message}" });
            }
        }


    }  
}