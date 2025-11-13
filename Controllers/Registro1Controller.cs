using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;
using System.Security.Cryptography;
using System.Text;

namespace proyecto_mejoradoMy_pet.Controllers
{
        public class Registro1Controller : Controller
        {
            private readonly BdMypetv3Context _context;

            public Registro1Controller(BdMypetv3Context context)
            {
                _context = context;
            }

            // GET: Registro
            public IActionResult Index()
            {
                var model = new RegistroCompleteViewModel();
                return View(model);
            }

        // Agregar estas mejoras al controlador:

        // 1. Modificar el método ProcesarRegistro para manejar JSON sin ValidateAntiForgeryToken
        [HttpPost]
        public async Task<IActionResult> ProcesarRegistro([FromBody] RegistroCompleteViewModel model)
        {
            try
            {
                // === [ INICIO DE VALIDACIONES PRELIMINARES ] ===
                System.Diagnostics.Debug.WriteLine($"Recibiendo datos: Usuario={model?.Usuario}");
                System.Diagnostics.Debug.WriteLine($"Roles seleccionados: {string.Join(",", model?.RolesSeleccionados ?? new List<string>())}");

                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    // ... (Lógica de Debug y retorno de errores de validación) ...
                    return Json(new { success = false, message = "Por favor corrige los errores en el formulario", errors = errors });
                }

                // Verificar unicidad
                if (await _context.TbUsuarios.AnyAsync(u => u.Usuario == model.Usuario))
                {
                    return Json(new { success = false, message = "El nombre de usuario ya existe" });
                }
                if (await _context.TbUsuarios.AnyAsync(u => u.Correo == model.Email))
                {
                    return Json(new { success = false, message = "El email ya está registrado" });
                }
                if (await _context.TbUsuarios.AnyAsync(u => u.DocumentoIdentidad == model.Documento))
                {
                    return Json(new { success = false, message = "El documento ya está registrado" });
                }

                // Conversión string a DateOnly
                if (!DateOnly.TryParse(model.FechaNacimiento, out DateOnly fechaNacimiento))
                {
                    return Json(new { success = false, message = "Fecha de nacimiento inválida" });
                }
                // === [ FIN DE VALIDACIONES PRELIMINARES ] ===

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Crear usuario
                    var usuario = new TbUsuario
                    {
                        Usuario = model.Usuario,
                        Password = HashPassword(model.Password),
                        Correo = model.Email,
                        PrimerNombre = model.PrimerNombre,
                        SegundoNombre = model.SegundoNombre,
                        PrimerApellido = model.PrimerApellido,
                        SegundoApellido = model.SegundoApellido,
                        Telefono = model.Telefono,
                        DocumentoIdentidad = model.Documento,
                        FechaNacimiento = fechaNacimiento,
                        FechaRegistro = DateTime.Now
                    };

                    _context.TbUsuarios.Add(usuario);
                    // GUARDA SÓLO EL USUARIO PARA OBTENER EL ID GENERADO
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Usuario creado con ID: {usuario.IdUsuario}");

                    // 2. Asignar roles (TbUsuarioRole)
                    foreach (var rolIdString in model.RolesSeleccionados) // rolIdString es STRING
                    {
                        if (int.TryParse(rolIdString, out int rolIdInt))
                        {
                            var rol = await _context.TbRoles.FirstOrDefaultAsync(r => r.IdRol == rolIdInt);

                            if (rol != null)
                            {
                                var usuarioRole = new TbUsuarioRole
                                {
                                    IdUsuario = usuario.IdUsuario, // Usa el ID generado
                                    IdRol = rol.IdRol
                                };
                                _context.TbUsuarioRoles.Add(usuarioRole);
                            }
                        }
                    }

                    // NOTA: Se eliminó un SaveChanges innecesario aquí.

                    // 3. Agregar direcciones (TbDireccione)
                    if (model.Direcciones != null && model.Direcciones.Any())
                    {
                        foreach (var direccionDto in model.Direcciones)
                        {
                            var direccion = new TbDireccione
                            {
                                IdUsuario = usuario.IdUsuario, // Usa el ID generado
                                Direccion = direccionDto.Direccion,
                                Ciudad = direccionDto.Ciudad,
                                Departamento = direccionDto.Departamento,
                                Pais = direccionDto.Pais,
                                EsPredeterminada = direccionDto.EsPredeterminada
                            };
                            _context.TbDirecciones.Add(direccion);
                            System.Diagnostics.Debug.WriteLine($"Dirección agregada: {direccion.Direccion}");
                        }
                    }

                    // 4. Si es prestador (ID = "2")
                    if (model.RolesSeleccionados?.Contains("2") == true && model.InfoPrestador != null)
                    {
                        var prestador = new TbPrestadore
                        {
                            IdUsuario = usuario.IdUsuario, // Usa el ID generado
                            Resumen = model.InfoPrestador.Resumen,
                            Habilidades = model.InfoPrestador.Habilidades,
                            Experiencia = model.InfoPrestador.Experiencia,
                            AñosExperiencia = model.InfoPrestador.AnosExperiencia,
                            CalificacionPromedio = 0
                        };

                        _context.TbPrestadores.Add(prestador);
                        // GUARDA SÓLO EL PRESTADOR PARA OBTENER EL ID GENERADO
                        await _context.SaveChangesAsync();

                        System.Diagnostics.Debug.WriteLine($"Prestador creado con ID: {prestador.IdPrestador}");

                        // Agregar servicios (TbServicio)
                        if (model.InfoPrestador.Servicios != null && model.InfoPrestador.Servicios.Any())
                        {
                            foreach (var servicioDto in model.InfoPrestador.Servicios)
                            {
                                var servicio = new TbServicio
                                {
                                    IdPrestador = prestador.IdPrestador, // Usa el ID generado
                                    Nombre = servicioDto.Nombre,
                                    Descripcion = servicioDto.Descripcion,
                                    Precio = servicioDto.Precio
                                };
                                _context.TbServicios.Add(servicio);
                                System.Diagnostics.Debug.WriteLine($"Servicio agregado: {servicio.Nombre}");
                            }
                        }

                        // Agregar disponibilidad (TbDisponibilidad)
                        if (model.InfoPrestador.Disponibilidad != null && model.InfoPrestador.Disponibilidad.Any())
                        {
                            foreach (var disponibilidadDto in model.InfoPrestador.Disponibilidad)
                            {
                                TimeOnly horaInicio = TimeOnly.Parse(disponibilidadDto.HoraInicio);
                                TimeOnly horaFin = TimeOnly.Parse(disponibilidadDto.HoraFin);

                                var disponibilidad = new TbDisponibilidad
                                {
                                    IdPrestador = prestador.IdPrestador, // Usa el ID generado
                                    DiaSemana = disponibilidadDto.DiaSemana,
                                    HoraInicio = horaInicio,
                                    HoraFin = horaFin
                                };
                                _context.TbDisponibilidads.Add(disponibilidad);
                                System.Diagnostics.Debug.WriteLine($"Disponibilidad agregada: {disponibilidad.DiaSemana} {horaInicio} - {horaFin}");
                            }
                        }
                    }

                    // 5. Agregar mascotas (ID de Dueño = "1")
                    // Condición de rol corregida para el Dueño
                    if (model.RolesSeleccionados?.Contains("1") == true && model.Mascotas != null && model.Mascotas.Any())
                    {
                        foreach (var mascotaDto in model.Mascotas)
                        {
                            var mascota = new TbMascota
                            {
                                IdUsuario = usuario.IdUsuario, // Usa el ID generado
                                Nombre = mascotaDto.Nombre,
                                Tipo = mascotaDto.Tipo,
                                Raza = mascotaDto.Raza,
                                Edad = mascotaDto.Edad
                            };
                            _context.TbMascotas.Add(mascota);
                            System.Diagnostics.Debug.WriteLine($"Mascota agregada: {mascota.Nombre}");
                        }
                    }

                    // 6. GUARDAR LOS CAMBIOS RESTANTES Y CONFIRMAR TRANSACCIÓN
                    // Esta llamada final guarda: Roles, Direcciones, Servicios, Disponibilidad y Mascotas.
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    System.Diagnostics.Debug.WriteLine("Registro completado exitosamente");
                    return Json(new { success = true, message = "Registro completado exitosamente" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"Error en transacción: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    // Propagar el error para que sea capturado por el catch externo y retorne un JSON de error.
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al procesar el registro: {ex.Message}" });
            }
        }

        // 2. Método auxiliar para validar token (opcional)
        private async Task ValidateAntiForgeryTokenAsync()
            {
                try
                {
                    var tokenSet = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
                    await tokenSet.ValidateRequestAsync(HttpContext);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error validando token: {ex.Message}");
                    // Puedes decidir si lanzar la excepción o continuar
                }
            }

            // Métodos auxiliares
            private byte[] HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }

            private Dictionary<string, List<string>> GetModelStateErrors()
            {
                var errors = new Dictionary<string, List<string>>();

                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state != null && state.Errors.Count > 0)
                    {
                        errors[key] = state.Errors.Select(e => e.ErrorMessage).ToList();
                    }
                }

                return errors;
            }

            // Endpoints AJAX para validaciones
            [HttpPost]
            public async Task<IActionResult> ValidarUsuario(string usuario)
            {
                var existe = await _context.TbUsuarios.AnyAsync(u => u.Usuario == usuario);
                return Json(new { existe });
            }

            [HttpPost]
            public async Task<IActionResult> ValidarEmail(string email)
            {
                var existe = await _context.TbUsuarios.AnyAsync(u => u.Correo == email);
                return Json(new { existe });
            }

            [HttpPost]
            public async Task<IActionResult> ValidarDocumento(string documento)
            {
                var existe = await _context.TbUsuarios.AnyAsync(u => u.DocumentoIdentidad == documento);
                return Json(new { existe });
            }

            #region Métodos de Utilidad

            // GET: Verificar disponibilidad de usuario
            [HttpGet]
            public async Task<IActionResult> VerificarUsuario(string usuario)
            {
                var existe = await _context.TbUsuarios.AnyAsync(u => u.Usuario == usuario);
                return Json(new { disponible = !existe });
            }

            // GET: Verificar disponibilidad de email
            [HttpGet]
            public async Task<IActionResult> VerificarEmail(string email)
            {
                var existe = await _context.TbUsuarios.AnyAsync(u => u.Correo == email);
                return Json(new { disponible = !existe });
            }

            // GET: Verificar disponibilidad de documento
            [HttpGet]
            public async Task<IActionResult> VerificarDocumento(string documento)
            {
                var existe = await _context.TbUsuarios.AnyAsync(u => u.DocumentoIdentidad == documento);
                return Json(new { disponible = !existe });
            }

            // GET: Obtener roles disponibles
            [HttpGet]
            public async Task<IActionResult> ObtenerRoles()
            {
                var roles = await _context.TbRoles.Select(r => new { r.IdRol, r.Nombre }).ToListAsync();
                return Json(roles);
            }

            #endregion
        }

    #region DTOs

    public class RegistroCompleteDto
    {
        // Información personal
        public string Usuario { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PrimerNombre { get; set; } = null!;
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string Telefono { get; set; } = null!;
        public string Documento { get; set; } = null!;
        public string FechaNacimiento { get; set; } = null!;
        public List<string> Roles { get; set; } = new();

        // Direcciones
        public List<DireccionDto> Direcciones { get; set; } = new();

        // Solo si es prestador
        public PrestadorDto? Prestador { get; set; }

        // Solo si es dueño
        public List<MascotaDto>? Mascotas { get; set; }
    }

    public class DireccionDto
    {
        public string Direccion { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public string Pais { get; set; } = null!;
        public bool EsPredeterminada { get; set; }
    }

    public class PrestadorDto
    {
        public string Resumen { get; set; } = null!;
        public string Habilidades { get; set; } = null!;
        public string? Experiencia { get; set; }
        public int AnosExperiencia { get; set; }
        public List<ServicioDto>? Servicios { get; set; }
        public List<DisponibilidadDto>? Disponibilidad { get; set; }
    }

    public class ServicioDto
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int Precio { get; set; }
    }

    public class DisponibilidadDto
    {
        public string DiaSemana { get; set; } = null!;
        public string HoraInicio { get; set; } = null!;
        public string HoraFin { get; set; } = null!;
    }

    public class MascotaDto
    {
        public string Nombre { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string? Raza { get; set; }
        public int? Edad { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public Dictionary<string, string> Errors { get; set; } = new();

        public void AddError(string field, string message)
        {
            Errors[field] = message;
        }
    }
    #endregion
}
