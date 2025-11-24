using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PedidodueñoController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PedidodueñoController(BdMypetv3Context context)
        {
            _context = context;
        }

        // GET: Pedidos/MisPedidos
        public async Task<IActionResult> MisPedidos()
        {
            // Verificar si el usuario está autenticado
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            // Obtener el ID del usuario desde la sesión
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Autenticacion");
            }

            // Obtener los pedidos del usuario que están en progreso o pendientes
            var pedidos = await _context.TbPedidos
                .Include(p => p.IdUsuarioPrestadorNavigation) // Incluir info del prestador
                .Include(p => p.IdDireccionNavigation) // Incluir dirección
                .Include(p => p.TbDetallePedidos) // Incluir detalles del pedido
                    .ThenInclude(d => d.IdServicioNavigation) // Incluir servicios
                .Include(p => p.TbPedidoMascota) // Incluir mascotas
                    .ThenInclude(pm => pm.IdMascotaNavigation)
                .Where(p => p.IdUsuarioDueño == userId &&
                           (p.Estado == "Pendiente" || p.Estado == "En Progreso"))
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            // Pasar información a la vista
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.IsAuthenticated = true;
            ViewBag.UserType = HttpContext.Session.GetString("UserType");

            return View(pedidos);
        }
    }
}
