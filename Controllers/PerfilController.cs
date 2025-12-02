using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PerfilController : Controller
    {
        private readonly BdMypetv3Context _context;

        public PerfilController(BdMypetv3Context context)
        {
            _context = context;
        }

        // Verificar autenticación
        private int? ObtenerIdUsuario()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out int userId))
                return userId;
            return null;
        }

        // GET: Perfil/EditarPerfil
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = ObtenerIdUsuario();
            if (userId == null)
                return RedirectToAction("Login", "Autenticacion");

            var usuario = await _context.TbUsuarios
                .Include(u => u.TbDirecciones)
                .FirstOrDefaultAsync(u => u.IdUsuario == userId.Value);

            if (usuario == null)
                return RedirectToAction("Login", "Autenticacion");

            var viewModel = new PerfilCompletoViewModel
            {
                InfoPersonal = new EditarInfoPersonalViewModel
                {
                    PrimerNombre = usuario.PrimerNombre,
                    SegundoNombre = usuario.SegundoNombre,
                    PrimerApellido = usuario.PrimerApellido,
                    SegundoApellido = usuario.SegundoApellido,
                    Telefono = usuario.Telefono,
                    Correo = usuario.Correo
                },
                Direcciones = usuario.TbDirecciones.ToList(),
                NombreCompleto = $"{usuario.PrimerNombre} {usuario.PrimerApellido}",
                Usuario = usuario.Usuario
            };

            ViewBag.UserName = usuario.PrimerNombre;
            ViewBag.IsAuthenticated = true;

            return View(viewModel);
        }

        // POST: Perfil/ActualizarInfoPersonal
        [HttpPost]
        public async Task<IActionResult> ActualizarInfoPersonal(EditarInfoPersonalViewModel model)
        {
            var userId = ObtenerIdUsuario();
            if (userId == null)
                return Json(new { success = false, message = "Sesión expirada" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            try
            {
                var usuario = await _context.TbUsuarios.FindAsync(userId.Value);
                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                // Verificar si el correo ya existe en otro usuario
                var correoExiste = await _context.TbUsuarios
                    .AnyAsync(u => u.Correo == model.Correo && u.IdUsuario != userId.Value);

                if (correoExiste)
                    return Json(new { success = false, message = "El correo ya está registrado por otro usuario" });

                usuario.PrimerNombre = model.PrimerNombre;
                usuario.SegundoNombre = model.SegundoNombre;
                usuario.PrimerApellido = model.PrimerApellido;
                usuario.SegundoApellido = model.SegundoApellido;
                usuario.Telefono = model.Telefono;
                usuario.Correo = model.Correo;

                await _context.SaveChangesAsync();

                // Actualizar sesión
                HttpContext.Session.SetString("UserName", usuario.PrimerNombre);

                return Json(new { success = true, message = "Información actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar: {ex.Message}" });
            }
        }

        // POST: Perfil/GuardarDireccion
        [HttpPost]
        public async Task<IActionResult> GuardarDireccion([FromBody] Direccion1ViewModel model)
        {
            var userId = ObtenerIdUsuario();
            if (userId == null)
                return Json(new { success = false, message = "Sesión expirada" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            try
            {
                // Si marca como predeterminada, desmarcar las demás
                if (model.EsPredeterminada)
                {
                    var direcciones = await _context.TbDirecciones
                        .Where(d => d.IdUsuario == userId.Value)
                        .ToListAsync();

                    foreach (var dir in direcciones)
                    {
                        dir.EsPredeterminada = false;
                    }
                }

                if (model.IdDireccion.HasValue && model.IdDireccion.Value > 0)
                {
                    // Editar existente
                    var direccion = await _context.TbDirecciones.FindAsync(model.IdDireccion.Value);
                    if (direccion == null || direccion.IdUsuario != userId.Value)
                        return Json(new { success = false, message = "Dirección no encontrada" });

                    direccion.Direccion = model.Direccion;
                    direccion.Ciudad = model.Ciudad;
                    direccion.Departamento = model.Departamento;
                    direccion.Pais = model.Pais;
                    direccion.EsPredeterminada = model.EsPredeterminada;
                }
                else
                {
                    // Crear nueva
                    var nuevaDireccion = new TbDireccione
                    {
                        IdUsuario = userId.Value,
                        Direccion = model.Direccion,
                        Ciudad = model.Ciudad,
                        Departamento = model.Departamento,
                        Pais = model.Pais,
                        EsPredeterminada = model.EsPredeterminada
                    };
                    _context.TbDirecciones.Add(nuevaDireccion);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Dirección guardada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al guardar: {ex.Message}" });
            }
        }

        // POST: Perfil/EliminarDireccion
        [HttpPost]
        public async Task<IActionResult> EliminarDireccion(int id)
        {
            var userId = ObtenerIdUsuario();
            if (userId == null)
                return Json(new { success = false, message = "Sesión expirada" });

            try
            {
                var direccion = await _context.TbDirecciones.FindAsync(id);
                if (direccion == null || direccion.IdUsuario != userId.Value)
                    return Json(new { success = false, message = "Dirección no encontrada" });

                // Verificar que no sea la única dirección
                var cantidadDirecciones = await _context.TbDirecciones
                    .CountAsync(d => d.IdUsuario == userId.Value);

                if (cantidadDirecciones <= 1)
                    return Json(new { success = false, message = "Debes tener al menos una dirección" });

                _context.TbDirecciones.Remove(direccion);
                await _context.SaveChangesAsync();

                // Si era predeterminada, marcar otra como predeterminada
                if (direccion.EsPredeterminada == true)
                {
                    var otraDireccion = await _context.TbDirecciones
                        .FirstOrDefaultAsync(d => d.IdUsuario == userId.Value);

                    if (otraDireccion != null)
                    {
                        otraDireccion.EsPredeterminada = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Dirección eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar: {ex.Message}" });
            }
        }

        // POST: Perfil/MarcarPredeterminada
        [HttpPost]
        public async Task<IActionResult> MarcarPredeterminada(int id)
        {
            var userId = ObtenerIdUsuario();
            if (userId == null)
                return Json(new { success = false, message = "Sesión expirada" });

            try
            {
                // Desmarcar todas
                var todasDirecciones = await _context.TbDirecciones
                    .Where(d => d.IdUsuario == userId.Value)
                    .ToListAsync();

                foreach (var dir in todasDirecciones)
                {
                    dir.EsPredeterminada = false;
                }

                // Marcar la seleccionada
                var direccion = todasDirecciones.FirstOrDefault(d => d.IdDireccion == id);
                if (direccion == null)
                    return Json(new { success = false, message = "Dirección no encontrada" });

                direccion.EsPredeterminada = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Dirección predeterminada actualizada" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Perfil/ObtenerDireccion/{id}
        [HttpGet]
        public async Task<IActionResult> ObtenerDireccion(int id)
        {
            var userId = ObtenerIdUsuario();
            if (userId == null)
                return Json(new { success = false, message = "Sesión expirada" });

            var direccion = await _context.TbDirecciones
                .Where(d => d.IdDireccion == id && d.IdUsuario == userId.Value)
                .Select(d => new
                {
                    d.IdDireccion,
                    d.Direccion,
                    d.Ciudad,
                    d.Departamento,
                    d.Pais,
                    d.EsPredeterminada
                })
                .FirstOrDefaultAsync();

            if (direccion == null)
                return Json(new { success = false, message = "Dirección no encontrada" });

            return Json(new { success = true, data = direccion });
        }
    }
}