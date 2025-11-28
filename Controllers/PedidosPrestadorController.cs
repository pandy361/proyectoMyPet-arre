using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PedidosPrestadorController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PedidosPrestadorController(BdMypetv3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Verificar si el usuario está autenticado y es prestador
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetString("UserId");

            if (isAuthenticated != "true" || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            // Obtener el nombre del prestador
            var nombrePrestador = HttpContext.Session.GetString("UserName");
            ViewBag.NombrePrestador = nombrePrestador;

            // Convertir el ID de sesión a int
            int idPrestador = int.Parse(userId);

            // Obtener todos los pedidos del prestador (incluyendo finalizados y rechazados)
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

            if (isAuthenticated != "true" || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "No autenticado" });
            }

            int idPrestador = int.Parse(userId);

            // Buscar el pedido y verificar que pertenezca al prestador
            var pedido = await _context.TbPedidos
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido && p.IdUsuarioPrestador == idPrestador);

            if (pedido == null)
            {
                return Json(new { success = false, message = "Pedido no encontrado o no autorizado" });
            }

            // Validar transiciones de estado válidas
            bool transicionValida = false;
            switch (pedido.Estado?.ToLower())
            {
                case "Pagado (Pendiente)":
                    transicionValida = nuevoEstado == "Aceptado" || nuevoEstado == "Rechazado";
                    break;
                case "aceptado":
                    transicionValida = nuevoEstado == "En Proceso" || nuevoEstado == "Finalizado";
                    break;
                case "en proceso":
                    transicionValida = nuevoEstado == "Finalizado";
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

            if (isAuthenticated != "true" || !userType.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            int idPrestador = int.Parse(userId);

            // Obtener el pedido con todos sus detalles
            var pedido = await _context.TbPedidos
                .Include(p => p.IdUsuarioDueñoNavigation)
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
                return NotFound();
            }

            ViewBag.NombrePrestador = HttpContext.Session.GetString("UserName");
            return View(pedido);
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Autenticacion");
        }
    }
}
