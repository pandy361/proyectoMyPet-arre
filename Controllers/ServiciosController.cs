using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly BdMypetv3Context _context;

        public ServiciosController(BdMypetv3Context context)
        {
            _context = context;
        }

        // Verificar autenticación
        private bool VerificarAutenticacion()
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var userType = HttpContext.Session.GetString("UserType");

            return isAuthenticated == "true" &&
                   (userType == "Prestador" || userType == "Admin");
        }

        // Obtener ID del prestador desde la sesión
        private async Task<int?> ObtenerIdPrestadorAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return null;

            var idUsuario = int.Parse(userId);

            // Buscar el prestador asociado a este usuario
            var prestador = await _context.TbPrestadores
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            return prestador?.IdPrestador;
        }

        // GET: Servicios - Vista principal
        public async Task<IActionResult> Index()
        {
            if (!VerificarAutenticacion())
                return RedirectToAction("Login", "Autenticacion");

            var userName = HttpContext.Session.GetString("UserName");
            var userType = HttpContext.Session.GetString("UserType");

            ViewBag.UserName = userName;
            ViewBag.UserType = userType;
            ViewBag.NombrePrestador = userName;

            List<TbServicio> servicios;

            // Si es prestador, solo ver sus servicios
            if (userType == "Prestador")
            {
                var idPrestador = await ObtenerIdPrestadorAsync();

                if (idPrestador == null)
                {
                    ViewBag.Error = "No se encontró el perfil de prestador";
                    return View(new List<TbServicio>());
                }

                servicios = await _context.TbServicios
                    .Where(s => s.IdPrestador == idPrestador.Value)
                    .Include(s => s.IdPrestadorNavigation)
                    .ToListAsync();
            }
            else
            {
                // Admin ve todos
                servicios = await _context.TbServicios
                    .Include(s => s.IdPrestadorNavigation)
                    .ToListAsync();
            }

            return View(servicios);
        }

        // POST: Servicios/Create - Crear nuevo servicio (AJAX)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TbServicio servicio)
        {
            if (!VerificarAutenticacion())
                return Unauthorized(new { success = false, message = "No autorizado" });

            try
            {
                // Obtener el ID del prestador
                var idPrestador = await ObtenerIdPrestadorAsync();

                if (idPrestador == null)
                {
                    return BadRequest(new { success = false, message = "No se encontró el perfil de prestador" });
                }

                // Asignar el ID del prestador
                servicio.IdPrestador = idPrestador.Value;
                servicio.IdServicio = 0; // Asegurar que sea nuevo

                // Validar datos básicos
                if (string.IsNullOrWhiteSpace(servicio.Nombre))
                {
                    return BadRequest(new { success = false, message = "El nombre del servicio es requerido" });
                }

                if (servicio.Precio <= 0)
                {
                    return BadRequest(new { success = false, message = "El precio debe ser mayor a cero" });
                }

                _context.TbServicios.Add(servicio);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Servicio creado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Servicios/Edit/5 - Editar servicio (AJAX)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] TbServicio servicio)
        {
            if (!VerificarAutenticacion())
                return Unauthorized(new { success = false, message = "No autorizado" });

            if (id != servicio.IdServicio)
                return BadRequest(new { success = false, message = "ID no coincide" });

            try
            {
                // Obtener el ID del prestador actual
                var idPrestadorActual = await ObtenerIdPrestadorAsync();
                var userType = HttpContext.Session.GetString("UserType");

                if (idPrestadorActual == null)
                {
                    return BadRequest(new { success = false, message = "No se encontró el perfil de prestador" });
                }

                // Buscar el servicio original
                var servicioOriginal = await _context.TbServicios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.IdServicio == id);

                if (servicioOriginal == null)
                    return NotFound(new { success = false, message = "Servicio no encontrado" });

                // Verificar que el prestador solo edite sus servicios
                if (userType == "Prestador" && servicioOriginal.IdPrestador != idPrestadorActual.Value)
                    return Forbid();

                // Validar datos
                if (string.IsNullOrWhiteSpace(servicio.Nombre))
                {
                    return BadRequest(new { success = false, message = "El nombre del servicio es requerido" });
                }

                if (servicio.Precio <= 0)
                {
                    return BadRequest(new { success = false, message = "El precio debe ser mayor a cero" });
                }

                // Mantener el IdPrestador original
                servicio.IdPrestador = servicioOriginal.IdPrestador;

                _context.TbServicios.Update(servicio);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Servicio actualizado exitosamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioExists(servicio.IdServicio))
                    return NotFound(new { success = false, message = "Servicio no encontrado" });
                else
                    throw;
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Servicios/Delete/5 - Eliminar servicio (AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!VerificarAutenticacion())
                return Unauthorized(new { success = false, message = "No autorizado" });

            try
            {
                var servicio = await _context.TbServicios.FindAsync(id);

                if (servicio == null)
                    return NotFound(new { success = false, message = "Servicio no encontrado" });

                // Obtener ID del prestador y verificar permisos
                var idPrestadorActual = await ObtenerIdPrestadorAsync();
                var userType = HttpContext.Session.GetString("UserType");

                if (idPrestadorActual == null)
                {
                    return BadRequest(new { success = false, message = "No se encontró el perfil de prestador" });
                }

                if (userType == "Prestador" && servicio.IdPrestador != idPrestadorActual.Value)
                    return Forbid();

                _context.TbServicios.Remove(servicio);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Servicio eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        private bool ServicioExists(int id)
        {
            return _context.TbServicios.Any(e => e.IdServicio == id);
        }
    }
}   