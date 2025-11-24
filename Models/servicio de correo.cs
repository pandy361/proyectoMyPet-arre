using System.Net;
using System.Net.Mail;

namespace proyecto_mejoradoMy_pet.Models
{
    public interface IEmailService
    {
        Task<bool> EnviarCorreoRecuperacionAsync(string correoDestino, string nombreUsuario, string enlaceRecuperacion);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarCorreoRecuperacionAsync(string correoDestino, string nombreUsuario, string enlaceRecuperacion)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                var mensaje = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "🐾 MyPet - Recuperación de Contraseña",
                    IsBodyHtml = true,
                    Body = GenerarPlantillaCorreo(nombreUsuario, enlaceRecuperacion)
                };

                mensaje.To.Add(correoDestino);

                await client.SendMailAsync(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                return false;
            }
        }

        private string GenerarPlantillaCorreo(string nombreUsuario, string enlaceRecuperacion)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f5f5f5; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 15px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 32px;'>🐾 MyPet</h1>
                            <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0 0;'>Recuperación de Contraseña</p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            <h2 style='color: #333; margin: 0 0 20px 0;'>Hola, {nombreUsuario} 👋</h2>
                            
                            <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                                Recibimos una solicitud para restablecer la contraseña de tu cuenta en MyPet.
                            </p>
                            
                            <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                                Haz clic en el siguiente botón para crear una nueva contraseña:
                            </p>
                            
                            <!-- Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' style='margin: 30px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{enlaceRecuperacion}' 
                                           style='display: inline-block; 
                                                  background: linear-gradient(135deg, #4CAF50, #2E7D32); 
                                                  color: #ffffff; 
                                                  padding: 15px 40px; 
                                                  text-decoration: none; 
                                                  border-radius: 10px; 
                                                  font-weight: bold;
                                                  font-size: 16px;'>
                                            Restablecer Contraseña
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style='color: #999; font-size: 14px; line-height: 1.6;'>
                                ⏰ Este enlace expirará en <strong>1 hora</strong>.
                            </p>
                            
                            <p style='color: #999; font-size: 14px; line-height: 1.6;'>
                                Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña permanecerá sin cambios.
                            </p>
                            
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            
                            <p style='color: #999; font-size: 12px;'>
                                Si el botón no funciona, copia y pega este enlace en tu navegador:<br>
                                <a href='{enlaceRecuperacion}' style='color: #667eea; word-break: break-all;'>{enlaceRecuperacion}</a>
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 25px; text-align: center;'>
                            <p style='color: #999; font-size: 12px; margin: 0;'>
                                © 2025 MyPet - Tu plataforma de confianza para el cuidado de mascotas
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
