using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

using System.Security.Cryptography;
using System.Text;

namespace proyecto_mejoradoMy_pet.Controllers
{
    public class PasswordController : Controller
    {
        private readonly BdMypetv3Context _context;
        private readonly IEmailService _emailService;

        public PasswordController(BdMypetv3Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Password/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // POST: Password/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Buscar usuario por correo
            var usuario = await _context.TbUsuarios
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == model.Correo.ToLower());

            // Si el correo no existe, mostrar error
            if (usuario == null)
            {
                ModelState.AddModelError("", "Este correo no está registrado en nuestra plataforma.");
                return View(model);
            }

            // Invalidar tokens anteriores del usuario
            var tokensAnteriores = await _context.TbPasswordResets
                .Where(t => t.IdUsuario == usuario.IdUsuario && !t.Usado)
                .ToListAsync();

            foreach (var token in tokensAnteriores)
            {
                token.Usado = true;
            }

            // Generar nuevo token
            var nuevoToken = GenerarToken();
            var passwordReset = new TbPasswordReset
            {
                IdUsuario = usuario.IdUsuario,
                Token = nuevoToken,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddHours(1), // Expira en 1 hora
                Usado = false
            };

            _context.TbPasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();

            // Generar enlace de recuperación
            var enlaceRecuperacion = Url.Action(
                "ResetPassword",
                "Password",
                new { token = nuevoToken },
                Request.Scheme
            );

            // Enviar correo
            var nombreCompleto = $"{usuario.PrimerNombre} {usuario.PrimerApellido}";
            await _emailService.EnviarCorreoRecuperacionAsync(
                usuario.Correo,
                nombreCompleto,
                enlaceRecuperacion
            );

            return View("ForgotPasswordConfirmation");
        }

        // GET: Password/ResetPassword?token=xxx
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Token no válido";
                return View("Error");
            }

            // Verificar token
            var resetToken = await _context.TbPasswordResets
                .Include(t => t.IdUsuarioNavigation)
                .FirstOrDefaultAsync(t => t.Token == token && !t.Usado);

            if (resetToken == null)
            {
                ViewBag.Error = "El enlace no es válido o ya fue utilizado.";
                return View("Error");
            }

            if (resetToken.FechaExpiracion < DateTime.Now)
            {
                ViewBag.Error = "El enlace ha expirado. Por favor solicita uno nuevo.";
                return View("Error");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model);
        }

        // POST: Password/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verificar token
            var resetToken = await _context.TbPasswordResets
                .Include(t => t.IdUsuarioNavigation)
                .FirstOrDefaultAsync(t => t.Token == model.Token && !t.Usado);

            if (resetToken == null)
            {
                ViewBag.Error = "El enlace no es válido o ya fue utilizado.";
                return View("Error");
            }

            if (resetToken.FechaExpiracion < DateTime.Now)
            {
                ViewBag.Error = "El enlace ha expirado. Por favor solicita uno nuevo.";
                return View("Error");
            }

            // Actualizar contraseña
            var usuario = resetToken.IdUsuarioNavigation;
            usuario.Password = HashPassword(model.NuevaPassword);

            // Marcar token como usado
            resetToken.Usado = true;

            await _context.SaveChangesAsync();

            return View("ResetPasswordConfirmation");
        }

        // Generar token seguro
        private string GenerarToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        // Hash de contraseña (mismo método que usas en Autenticación)
        private byte[] HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}