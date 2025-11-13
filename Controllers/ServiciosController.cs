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

        // GET: Servicios - Vista principal
        public async Task<IActionResult> Index()
        {
            if (!VerificarAutenticacion())
                return RedirectToAction("Login", "Autenticacion");

            var userId = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var userType = HttpContext.Session.GetString("UserType");

            ViewBag.UserName = userName;
            ViewBag.UserType = userType;

            List<TbServicio> servicios;

            // Si es prestador, solo ver sus servicios
            if (userType == "Prestador")
            {
                var idPrestador = int.Parse(userId);
                servicios = await _context.TbServicios
                    .Where(s => s.IdPrestador == idPrestador)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] TbServicio servicio)
        {
            if (!VerificarAutenticacion())
                return Unauthorized();

            try
            {
                // Asignar el ID del prestador logueado
                var userId = HttpContext.Session.GetString("UserId");
                servicio.IdPrestador = int.Parse(userId);

                _context.Add(servicio);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromBody] TbServicio servicio)
        {
            if (!VerificarAutenticacion())
                return Unauthorized();

            if (id != servicio.IdServicio)
                return BadRequest();

            try
            {
                // Verificar que el prestador solo edite sus servicios
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");

                var servicioOriginal = await _context.TbServicios.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.IdServicio == id);

                if (servicioOriginal == null)
                    return NotFound();

                if (userType == "Prestador" && servicioOriginal.IdPrestador != int.Parse(userId))
                    return Forbid();

                // Mantener el IdPrestador original
                servicio.IdPrestador = servicioOriginal.IdPrestador;

                _context.Update(servicio);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Servicio actualizado exitosamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicioExists(servicio.IdServicio))
                    return NotFound();
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!VerificarAutenticacion())
                return Unauthorized();

            try
            {
                var servicio = await _context.TbServicios.FindAsync(id);

                if (servicio == null)
                    return NotFound();

                // Verificar permisos
                var userId = HttpContext.Session.GetString("UserId");
                var userType = HttpContext.Session.GetString("UserType");

                if (userType == "Prestador" && servicio.IdPrestador != int.Parse(userId))
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
