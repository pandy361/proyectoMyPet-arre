using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Filters;
using proyecto_mejoradoMy_pet.Models;

namespace proyecto_mejoradoMy_pet.Controllers
{
    [PreventPageReload]
    public class PrestadoresController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PrestadoresController(BdMypetv3Context context)
        {
            _context = context;
        }

        // GET: Prestadores
        public async Task<IActionResult> Index(string buscar = "", string filtro = "all", int pagina = 1)
        {
            const int tamanoPagina = 9;

            var query = _context.TbPrestadores
                .Include(p => p.IdUsuarioNavigation)
                    .ThenInclude(u => u.TbUsuarioImagenes)
                .Include(p => p.TbServicios)
                .AsQueryable();

            // ✅ FILTRO DE BÚSQUEDA
            if (!string.IsNullOrEmpty(buscar))
            {
                buscar = buscar.ToLower().Trim();
                query = query.Where(p =>
                    p.IdUsuarioNavigation.PrimerNombre.ToLower().Contains(buscar) ||
                    p.IdUsuarioNavigation.PrimerApellido.ToLower().Contains(buscar) ||
                    (p.Habilidades != null && p.Habilidades.ToLower().Contains(buscar)) ||
                    (p.ServiciosOfrecidos != null && p.ServiciosOfrecidos.ToLower().Contains(buscar)) ||
                    p.TbServicios.Any(s => s.Nombre.ToLower().Contains(buscar))
                );
            }

            // ✅ FILTROS POR CATEGORÍA
            switch (filtro.ToLower())
            {
                case "top-rated":
                    // Prestadores con calificación >= 4.5
                    query = query.Where(p => p.CalificacionPromedio >= 4.5m);
                    break;

                case "veterinary":
                    // Prestadores que ofrecen servicios veterinarios
                    query = query.Where(p =>
                        (p.Habilidades != null && p.Habilidades.ToLower().Contains("veterinari")) ||
                        (p.ServiciosOfrecidos != null && p.ServiciosOfrecidos.ToLower().Contains("veterinari")) ||
                        p.TbServicios.Any(s => s.Nombre.ToLower().Contains("veterinari"))
                    );
                    break;

                case "grooming":
                    // Prestadores que ofrecen peluquería/grooming
                    query = query.Where(p =>
                        (p.Habilidades != null && (
                            p.Habilidades.ToLower().Contains("peluquer") ||
                            p.Habilidades.ToLower().Contains("grooming") ||
                            p.Habilidades.ToLower().Contains("baño") ||
                            p.Habilidades.ToLower().Contains("corte")
                        )) ||
                        (p.ServiciosOfrecidos != null && (
                            p.ServiciosOfrecidos.ToLower().Contains("peluquer") ||
                            p.ServiciosOfrecidos.ToLower().Contains("grooming") ||
                            p.ServiciosOfrecidos.ToLower().Contains("baño") ||
                            p.ServiciosOfrecidos.ToLower().Contains("corte")
                        )) ||
                        p.TbServicios.Any(s =>
                            s.Nombre.ToLower().Contains("peluquer") ||
                            s.Nombre.ToLower().Contains("grooming") ||
                            s.Nombre.ToLower().Contains("baño") ||
                            s.Nombre.ToLower().Contains("corte")
                        )
                    );
                    break;

                case "walking":
                    // Prestadores que ofrecen paseos
                    query = query.Where(p =>
                        (p.Habilidades != null && (
                            p.Habilidades.ToLower().Contains("paseo") ||
                            p.Habilidades.ToLower().Contains("walk")
                        )) ||
                        (p.ServiciosOfrecidos != null && (
                            p.ServiciosOfrecidos.ToLower().Contains("paseo") ||
                            p.ServiciosOfrecidos.ToLower().Contains("walk")
                        )) ||
                        p.TbServicios.Any(s =>
                            s.Nombre.ToLower().Contains("paseo") ||
                            s.Nombre.ToLower().Contains("walk")
                        )
                    );
                    break;

                case "training":
                    // Prestadores que ofrecen entrenamiento
                    query = query.Where(p =>
                        (p.Habilidades != null && (
                            p.Habilidades.ToLower().Contains("entrenamient") ||
                            p.Habilidades.ToLower().Contains("adiestramiento") ||
                            p.Habilidades.ToLower().Contains("training")
                        )) ||
                        (p.ServiciosOfrecidos != null && (
                            p.ServiciosOfrecidos.ToLower().Contains("entrenamient") ||
                            p.ServiciosOfrecidos.ToLower().Contains("adiestramiento") ||
                            p.ServiciosOfrecidos.ToLower().Contains("training")
                        )) ||
                        p.TbServicios.Any(s =>
                            s.Nombre.ToLower().Contains("entrenamient") ||
                            s.Nombre.ToLower().Contains("adiestramiento") ||
                            s.Nombre.ToLower().Contains("training")
                        )
                    );
                    break;

                case "all":
                default:
                    // Mostrar todos
                    break;
            }

            // ✅ ORDENAR: Mejor valorados primero
            query = query.OrderByDescending(p => p.CalificacionPromedio ?? 0)
                         .ThenBy(p => p.IdUsuarioNavigation.PrimerNombre);

            // Calcular paginación
            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalItems / tamanoPagina);

            var prestadores = await query
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            // ViewBag para paginación y filtros
            ViewBag.BuscarTermino = buscar;
            ViewBag.FiltroActual = filtro;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalResultados = totalItems;

            // ViewBag para autenticación
            ViewBag.IsAuthenticated = HttpContext.Session.GetString("IsAuthenticated") == "true";
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

            // Información de sesión
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            ViewBag.IsAuthenticated = isAuthenticated == "true";
            ViewBag.UserType = HttpContext.Session.GetString("UserType") ?? "Invitado";
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Usuario";

            return View(prestador);
        }
    }
}