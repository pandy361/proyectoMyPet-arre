using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace proyecto_mejoradoMy_pet.Filters
{
    public class PreventPageReloadAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var request = context.HttpContext.Request;

            // Verificar si hay sesión activa
            var isAuthenticated = session.GetString("IsAuthenticated");

            if (isAuthenticated != "true")
            {
                base.OnActionExecuting(context);
                return;
            }

            // Verificar si viene del header especial que agregamos con JavaScript
            var isReload = request.Headers["X-Is-Reload"].ToString();

            if (isReload == "true")
            {
                System.Diagnostics.Debug.WriteLine("[RECARGA DETECTADA] Cerrando sesión...");

                // Cerrar sesión
                session.Clear();

                // Redirigir a login
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Autenticacion",
                    new { mensaje = "Tu sesión ha sido cerrada por seguridad (recarga detectada)" }
                );
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}