using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;
using System.Security.Cryptography;
using System.Text;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly BdMypetv3Context _context;

        public AutenticacionController(BdMypetv3Context context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new UserLoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            // LOG 1: Ver qué datos llegan
            Console.WriteLine("===== INICIO LOGIN =====");
            Console.WriteLine($"Usuario ingresado: {model.Usuario}");
            Console.WriteLine($"Rol seleccionado: {model.Rol}");
            Console.WriteLine($"ModelState válido: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState NO válido:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                // 1. Buscar el usuario
                Console.WriteLine($"🔍 Buscando usuario: {model.Usuario}");
                var usuario = await _context.TbUsuarios
                    .FirstOrDefaultAsync(u => u.Usuario == model.Usuario);

                if (usuario == null)
                {
                    Console.WriteLine("❌ Usuario no encontrado en BD");
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                Console.WriteLine($"✅ Usuario encontrado: ID={usuario.IdUsuario}, Nombre={usuario.PrimerNombre}");

                // 2. Verificar contraseña
                var hashIngresado = HashPassword(model.Contrasena);
                Console.WriteLine($"🔐 Verificando contraseña...");

                if (!usuario.Password.SequenceEqual(hashIngresado))
                {
                    Console.WriteLine("❌ Contraseña incorrecta");
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                Console.WriteLine("✅ Contraseña correcta");

                // 3. ✅ VERIFICAR ROL - Usando Include correctamente
                Console.WriteLine($"🔍 Verificando rol '{model.Rol}' para usuario ID={usuario.IdUsuario}");

                // Obtener roles del usuario usando Include
                var rolesUsuario = await _context.TbUsuarioRoles
                    .Where(ur => ur.IdUsuario == usuario.IdUsuario)
                    .Include(ur => ur.IdRolNavigation)
                    .Select(ur => ur.IdRolNavigation.Nombre)
                    .ToListAsync();

                Console.WriteLine($"📋 Roles del usuario en BD: [{string.Join(", ", rolesUsuario)}]");

                if (!rolesUsuario.Any())
                {
                    Console.WriteLine("❌ ERROR: Usuario no tiene ningún rol asignado");
                    ModelState.AddModelError("", "Tu cuenta no tiene roles asignados. Contacta al administrador.");
                    return View(model);
                }

                // ✅ NORMALIZAR y comparar (quitar espacios, tildes, mayúsculas)
                string rolNormalizado = NormalizarTexto(model.Rol);

                // Mostrar comparación detallada
                Console.WriteLine($"   Rol solicitado: '{model.Rol}' → normalizado: '{rolNormalizado}'");
                foreach (var rol in rolesUsuario)
                {
                    Console.WriteLine($"   Rol en BD: '{rol}' → normalizado: '{NormalizarTexto(rol)}'");
                }

                bool tieneRol = rolesUsuario.Any(r => NormalizarTexto(r) == rolNormalizado);
                Console.WriteLine($"   ¿Tiene el rol '{model.Rol}'? {(tieneRol ? "SÍ ✅" : "NO ❌")}");

                if (!tieneRol)
                {
                    Console.WriteLine($"❌ ERROR: Usuario no tiene permiso para rol '{model.Rol}'");
                    ModelState.AddModelError("", $"No tienes permisos para acceder como {model.Rol}.");
                    return View(model);
                }

                Console.WriteLine("✅ Rol verificado correctamente");

                // 4. Guardar información del usuario en sesión
                Console.WriteLine("💾 Guardando sesión...");
                HttpContext.Session.SetString("UserId", usuario.IdUsuario.ToString());
                HttpContext.Session.SetString("UserName", usuario.PrimerNombre);
                HttpContext.Session.SetString("UserType", model.Rol);
                HttpContext.Session.SetString("IsAuthenticated", "true");
                Console.WriteLine($"   UserId: {usuario.IdUsuario}");
                Console.WriteLine($"   UserName: {usuario.PrimerNombre}");
                Console.WriteLine($"   UserType: {model.Rol}");
                Console.WriteLine("✅ Sesión guardada");

                // 5. Redirigir según el rol
                string rolNormalizadoFinal = NormalizarTexto(model.Rol);

                if (rolNormalizadoFinal == "dueno")
                {
                    Console.WriteLine($"🎯 Redirigiendo a: Prestadores/Index");
                    Console.WriteLine("===== FIN LOGIN EXITOSO =====\n");
                    return RedirectToAction("Index", "Prestadores");
                }

                if (rolNormalizadoFinal == "prestador")
                {
                    Console.WriteLine($"🎯 Redirigiendo a: PedidosPrestador/Index");
                    Console.WriteLine("===== FIN LOGIN EXITOSO =====\n");
                    return RedirectToAction("Index", "PedidosPrestador");
                }

                Console.WriteLine($"🎯 Redirigiendo a: Home/Index");
                Console.WriteLine("===== FIN LOGIN EXITOSO =====\n");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌❌❌ EXCEPCIÓN: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                Console.WriteLine("===== FIN LOGIN CON ERROR =====\n");

                ModelState.AddModelError("", "Error al iniciar sesión. Intenta nuevamente.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Autenticacion");
        }

        private byte[] HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // ✅ MÉTODO: Normalizar texto para comparación (sin tildes, minúsculas, sin espacios)
        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            // Convertir a minúsculas, quitar espacios y tildes
            return texto.ToLowerInvariant()
                       .Trim()
                       .Replace("ñ", "n")
                       .Replace("á", "a")
                       .Replace("é", "e")
                       .Replace("í", "i")
                       .Replace("ó", "o")
                       .Replace("ú", "u");
        }
    }
}