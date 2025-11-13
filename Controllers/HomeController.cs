using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;
using System.Diagnostics;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class HomeController : Controller
    {
        private readonly BdMypetv3Context _context;

        public HomeController(BdMypetv3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = await GetHomeViewModelAsync();
            return View(model);
        }

        public async Task<IActionResult> Privacy()
        {
            var model = await GetHomeViewModelAsync();
            return View(model);
        }

        private async Task<HomeViewModel> GetHomeViewModelAsync()
        {
            try
            {
                // Obtener estadísticas reales de la base de datos
                var statistics = new StatisticsViewModel
                {
                    TotalUsuarios = await _context.TbUsuarios.CountAsync(),
                    TotalPrestadores = await _context.TbPrestadores.CountAsync(),
                    TotalPedidos = await _context.TbPedidos.CountAsync(),
                    TotalMascotas = await _context.TbMascotas.CountAsync()
                };

                // Calcular promedio de calificaciones si hay reseñas
                var promedioCalificaciones = await _context.TbReseñas
                    .Where(r => r.Calificacion.HasValue && r.Calificacion > 0)
                    .AverageAsync(r => (double)(r.Calificacion ?? 0));

                statistics.PromedioCalificaciones = promedioCalificaciones > 0 ? promedioCalificaciones : 4.8;

                // Obtener servicios populares (ordenados por precio o por cantidad de pedidos)
                var popularServices = await _context.TbServicios
                    .Include(s => s.IdPrestadorNavigation)
                    .ThenInclude(p => p.IdUsuarioNavigation)
                    .Where(s => s.IdPrestadorNavigation != null && s.IdPrestadorNavigation.IdUsuarioNavigation != null)
                    .OrderByDescending(s => s.Precio)
                    .Take(6)
                    .Select(s => new ServiceViewModel
                    {
                        Id = s.IdServicio,
                        Nombre = s.Nombre ?? "",
                        Descripcion = s.Descripcion,
                        Precio = s.Precio,
                        PrestadorNombre = $"{s.IdPrestadorNavigation.IdUsuarioNavigation.PrimerNombre} {s.IdPrestadorNavigation.IdUsuarioNavigation.PrimerApellido}"
                    })
                    .ToListAsync();

                // Obtener prestadores mejor calificados
                var topProviders = await _context.TbPrestadores
                    .Include(p => p.IdUsuarioNavigation)
                    .Where(p => p.IdUsuarioNavigation != null)
                    .OrderByDescending(p => p.CalificacionPromedio ?? 0)
                    .ThenByDescending(p => p.AñosExperiencia ?? 0)
                    .Take(6)
                    .Select(p => new ProviderViewModel
                    {
                        Id = p.IdPrestador,
                        NombreCompleto = $"{p.IdUsuarioNavigation.PrimerNombre} {p.IdUsuarioNavigation.PrimerApellido}",
                        Especialidad = p.Resumen ?? "Prestador de servicios para mascotas",
                        CalificacionPromedio = p.CalificacionPromedio ?? 0,
                        AñosExperiencia = p.AñosExperiencia ?? 0,
                        Resumen = p.Resumen
                    })
                    .ToListAsync();

                // Obtener reseñas más recientes
                var recentReviews = await _context.TbReseñas
                    .Include(r => r.IdUsuarioNavigation)
                    .Include(r => r.IdPedidoNavigation)
                        .ThenInclude(p => p.IdUsuarioPrestadorNavigation)
                    .Where(r => r.IdUsuarioNavigation != null &&
                               r.IdPedidoNavigation != null &&
                               r.IdPedidoNavigation.IdUsuarioPrestadorNavigation != null &&
                               !string.IsNullOrEmpty(r.Comentario))
                    .OrderByDescending(r => r.Fecha ?? DateTime.Now)
                    .Take(6)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.IdReseña,
                        ClienteNombre = $"{r.IdUsuarioNavigation.PrimerNombre} {r.IdUsuarioNavigation.PrimerApellido}",
                        PrestadorNombre = $"{r.IdPedidoNavigation.IdUsuarioPrestadorNavigation.PrimerNombre} {r.IdPedidoNavigation.IdUsuarioPrestadorNavigation.PrimerApellido}",
                        Comentario = r.Comentario ?? "",
                        Calificacion = r.Calificacion ?? 5,
                        Fecha = r.Fecha ?? DateTime.Now
                    })
                    .ToListAsync();

                // Obtener tipos de mascotas más populares
                var popularPetTypes = await _context.TbMascotas
                    .Where(m => !string.IsNullOrEmpty(m.Tipo))
                    .GroupBy(m => m.Tipo)
                    .Select(g => new PetTypeViewModel
                    {
                        Tipo = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(pt => pt.Cantidad)
                    .Take(5)
                    .ToListAsync();

                return new HomeViewModel
                {
                    Statistics = statistics,
                    PopularServices = popularServices,
                    TopProviders = topProviders,
                    RecentReviews = recentReviews,
                    PopularPetTypes = popularPetTypes
                };
            }
            catch (Exception ex)
            {
                // Log el error y devolver un modelo con datos por defecto
                Console.WriteLine($"Error al obtener datos del HomeViewModel: {ex.Message}");

                return new HomeViewModel
                {
                    Statistics = new StatisticsViewModel
                    {
                        TotalUsuarios = 0,
                        TotalPrestadores = 0,
                        TotalPedidos = 0,
                        TotalMascotas = 0,
                        PromedioCalificaciones = 4.8
                    },
                    PopularServices = new List<ServiceViewModel>(),
                    TopProviders = new List<ProviderViewModel>(),
                    RecentReviews = new List<ReviewViewModel>(),
                    PopularPetTypes = new List<PetTypeViewModel>()
                };
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
