using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PrestadoresController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PrestadoresController(BdMypetv3Context context)
        {
            _context = context;
        }

        // GET: Prestadores
        public async Task<IActionResult> Index(string buscar = "", int pagina = 1)
        {
            const int tamanoPagina = 9;
            var query = _context.TbPrestadores
                .Include(p => p.IdUsuarioNavigation)
                    .ThenInclude(u => u.TbUsuarioImagenes)
                .Include(p => p.TbServicios)
                .AsQueryable();

            // Filtro de búsqueda
            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(p =>
                    p.IdUsuarioNavigation.PrimerNombre.Contains(buscar) ||
                    p.IdUsuarioNavigation.PrimerApellido.Contains(buscar) ||
                    p.Habilidades.Contains(buscar) ||
                    p.ServiciosOfrecidos.Contains(buscar) ||
                    p.TbServicios.Any(s => s.Nombre.Contains(buscar))
                );
            }

            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalItems / tamanoPagina);

            var prestadores = await query
                .OrderByDescending(p => p.CalificacionPromedio)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            // ViewBag para paginación
            ViewBag.BuscarTermino = buscar;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            // ViewBag para autenticación (para la sidebar)
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
            ViewBag.UserType = HttpContext.Session.GetString("UserType") ?? "Invitado";
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Usuario";

            return View(prestadores);
        }

        // GET: Prestadores/Details/5
        public async Task<IActionResult> Details(int id)
        {

            var sessionDebug = new
            {
                IsAuth = HttpContext.Session.GetString("IsAuthenticated"),
                UserId = HttpContext.Session.GetString("UserId"),
                UserType = HttpContext.Session.GetString("UserType"),
                UserName = HttpContext.Session.GetString("UserName")
            };
            Console.WriteLine($"DEBUG Session: IsAuth={sessionDebug.IsAuth}, UserId={sessionDebug.UserId}, Type={sessionDebug.UserType}, Name={sessionDebug.UserName}");
            var prestador = await _context.TbPrestadores
                .Include(p => p.IdUsuarioNavigation)
                    .ThenInclude(u => u.TbUsuarioImagenes)
                .Include(p => p.TbServicios)
                .Include(p => p.TbDisponibilidads)
                .Include(p => p.IdUsuarioNavigation)
                    .ThenInclude(u => u.TbReseñas)
                        .ThenInclude(r => r.IdPedidoNavigation)
                .FirstOrDefaultAsync(p => p.IdPrestador == id);

            if (prestador == null)
            {
                return NotFound();
            }

            // Calcular estadísticas
            var reseñas = prestador.IdUsuarioNavigation.TbReseñas.ToList();
            ViewBag.TotalReseñas = reseñas.Count;
            ViewBag.PromedioCalificacion = reseñas.Any() ? reseñas.Average(r => r.Calificacion ?? 0) : 0;

            // AGREGAR ESTO: Pasar información de sesión al ViewBag
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            ViewBag.IsAuthenticated = isAuthenticated == "true";
            ViewBag.UserType = HttpContext.Session.GetString("UserType") ?? "Invitado";
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Usuario";

            return View(prestador);
        }
    }
}
