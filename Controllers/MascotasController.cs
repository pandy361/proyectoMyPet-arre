using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

public class MascotasController : Controller
{
    private readonly BdMypetv3Context _context;

    public MascotasController(BdMypetv3Context context)
    {
        _context = context;
    }

    // Método auxiliar para obtener el ID del usuario autenticado
    private int? ObtenerUsuarioId()
    {
        // Obtener el UserId de la sesión (guardado como string)
        var userIdString = HttpContext.Session.GetString("UserId");

        if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
        {
            return userId;
        }

        return null;
    }

    // Método auxiliar para obtener el nombre del usuario
    private string ObtenerNombreUsuario()
    {
        return HttpContext.Session.GetString("UserName") ?? "Usuario";
    }

    // Método auxiliar para verificar si está autenticado
    private bool EstaAutenticado()
    {
        var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
        return isAuthenticated == "true";
    }

    // GET: Página principal
    public async Task<IActionResult> Index()
    {
        if (!EstaAutenticado())
        {
            return RedirectToAction("Login", "Autenticacion");
        }

        var usuarioId = ObtenerUsuarioId();

        if (usuarioId == null)
        {
            return RedirectToAction("Login", "Autenticacion");
        }

        // Obtener información del usuario para mostrar en la vista
        var usuario = await _context.TbUsuarios
            .Where(u => u.IdUsuario == usuarioId.Value)
            .Select(u => new {
                NombreCompleto = u.PrimerNombre + " " + u.PrimerApellido,
                Usuario = u.Usuario
            })
            .FirstOrDefaultAsync();

        if (usuario != null)
        {
            ViewBag.NombreUsuario = usuario.NombreCompleto;
            ViewBag.Usuario = usuario.Usuario;
        }
        else
        {
            // Si no encuentra el usuario en BD, usar el nombre de la sesión
            ViewBag.NombreUsuario = ObtenerNombreUsuario();
        }

        return View();
    }

    // GET: Obtener todas las mascotas del usuario
    [HttpGet]
    public async Task<IActionResult> ObtenerMascotas()
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var mascotas = await _context.TbMascotas
                .Where(m => m.IdUsuario == usuarioId.Value)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.IdMascota,
                    m.Nombre,
                    m.Tipo,
                    m.Raza,
                    m.Edad
                })
                .ToListAsync();

            return Json(new { success = true, data = mascotas, usuarioId = usuarioId.Value });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener mascotas: {ex.Message}");
            return Json(new { success = false, message = "Error al cargar las mascotas" });
        }
    }

    // POST: Crear nueva mascota
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] MascotaDto model)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var mascota = new TbMascota
            {
                IdUsuario = usuarioId.Value,
                Nombre = model.Nombre,
                Tipo = model.Tipo,
                Raza = model.Raza,
                Edad = model.Edad
            };

            _context.TbMascotas.Add(mascota);
            await _context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"Mascota creada: {mascota.Nombre} (ID: {mascota.IdMascota})");

            return Json(new { success = true, message = $"¡{mascota.Nombre} ha sido agregado exitosamente!" });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al crear mascota: {ex.Message}");
            return Json(new { success = false, message = "Error al crear la mascota" });
        }
    }

    // POST: Actualizar mascota
    [HttpPost]
    public async Task<IActionResult> Actualizar([FromBody] MascotaDto model)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var mascota = await _context.TbMascotas
                .FirstOrDefaultAsync(m => m.IdMascota == model.IdMascota && m.IdUsuario == usuarioId.Value);

            if (mascota == null)
            {
                return Json(new { success = false, message = "Mascota no encontrada" });
            }

            mascota.Nombre = model.Nombre;
            mascota.Tipo = model.Tipo;
            mascota.Raza = model.Raza;
            mascota.Edad = model.Edad;

            await _context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"Mascota actualizada: {mascota.Nombre} (ID: {mascota.IdMascota})");

            return Json(new { success = true, message = $"¡{mascota.Nombre} ha sido actualizado exitosamente!" });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al actualizar mascota: {ex.Message}");
            return Json(new { success = false, message = "Error al actualizar la mascota" });
        }
    }

    // DELETE: Eliminar mascota
    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            var mascota = await _context.TbMascotas
                .FirstOrDefaultAsync(m => m.IdMascota == id && m.IdUsuario == usuarioId.Value);

            if (mascota == null)
            {
                return Json(new { success = false, message = "Mascota no encontrada" });
            }

            // Verificar si la mascota tiene pedidos asociados
            var tienePedidos = await _context.TbPedidoMascotas
                .AnyAsync(pm => pm.IdMascota == id);

            if (tienePedidos)
            {
                return Json(new
                {
                    success = false,
                    message = "No se puede eliminar esta mascota porque tiene pedidos asociados"
                });
            }

            var nombreMascota = mascota.Nombre;
            _context.TbMascotas.Remove(mascota);
            await _context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"Mascota eliminada: {nombreMascota} (ID: {id})");

            return Json(new { success = true, message = $"{nombreMascota} ha sido eliminado" });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar mascota: {ex.Message}");
            return Json(new { success = false, message = "Error al eliminar la mascota" });
        }
    }

    // DTO para recibir datos
    public class MascotaDto
    {
        public int IdMascota { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public int? Edad { get; set; }
    }
}