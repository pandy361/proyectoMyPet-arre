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
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // 1. Primero buscar el usuario (sin Include para evitar errores)
                var usuario = await _context.TbUsuarios
                    .FirstOrDefaultAsync(u => u.Usuario == model.Usuario);

                if (usuario == null)
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                // 2. Verificar contraseña
                var hashIngresado = HashPassword(model.Contrasena);
                if (!usuario.Password.SequenceEqual(hashIngresado))
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                // 3. verificar rol

                var tieneRol = await _context.TbRoles
    .AnyAsync(r => r.Nombre.Equals(model.Rol) &&
                    r.TbUsuarioRoles.Any(ur => ur.IdUsuario == usuario.IdUsuario));

                // 4. Guardar información del usuario en sesión
                HttpContext.Session.SetString("UserId", usuario.IdUsuario.ToString());
                HttpContext.Session.SetString("UserName", usuario.PrimerNombre);
                HttpContext.Session.SetString("UserType", model.Rol);
                HttpContext.Session.SetString("IsAuthenticated", "true");

                // 5. Redirigir según el rol
                if (model.Rol.Equals("Dueño", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Prestadores");
                if (model.Rol.Equals("Prestador", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "PedidosPrestador");
                


                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Para debug - quitar en producción
                ModelState.AddModelError("", $"Error específico: {ex.Message}");
                Console.WriteLine($"Error completo: {ex}");
                return View(model);
            }
        }

        // Método auxiliar para comparar passwords
        private bool CompararPasswords(byte[] storedHash, byte[] inputHash)
        {
            if (storedHash.Length != inputHash.Length)
                return false;

            for (int i = 0; i < storedHash.Length; i++)
            {
                if (storedHash[i] != inputHash[i])
                    return false;
            }
            return true;
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
        
    }
}
