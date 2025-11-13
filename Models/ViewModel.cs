using System.ComponentModel.DataAnnotations;

namespace proyecto_mejoradoMy_pet.Models
{
    
    
        public class HomeViewModel
        {
            public List<ServiceViewModel> PopularServices { get; set; } = new List<ServiceViewModel>();
            public List<ProviderViewModel> TopProviders { get; set; } = new List<ProviderViewModel>();
            public List<ReviewViewModel> RecentReviews { get; set; } = new List<ReviewViewModel>();
            public StatisticsViewModel Statistics { get; set; } = new StatisticsViewModel();
            public List<PetTypeViewModel> PopularPetTypes { get; set; } = new List<PetTypeViewModel>();
        }

        public class ServiceViewModel
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public int Precio { get; set; }
            public string PrestadorNombre { get; set; } = string.Empty;
            public string PrecioFormateado => $"${Precio:N0}";
            public string IconoServicio
            {
                get
                {
                    return Nombre?.ToLower() switch
                    {
                        string n when n.Contains("paseo") || n.Contains("caminar") => "walking",
                        string n when n.Contains("peluqueria") || n.Contains("baño") || n.Contains("spa") => "grooming",
                        string n when n.Contains("cuidado") || n.Contains("casa") => "boarding",
                        string n when n.Contains("entrenamiento") || n.Contains("adiestramiento") => "training",
                        _ => "walking"
                    };
                }
            }
        }

        public class ProviderViewModel
        {
            public int Id { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public string? Especialidad { get; set; }
            public decimal CalificacionPromedio { get; set; }
            public int AñosExperiencia { get; set; }
            public string? Resumen { get; set; }
            public string Iniciales
            {
                get
                {
                    var nombres = NombreCompleto.Split(' ');
                    if (nombres.Length >= 2)
                        return $"{nombres[0][0]}{nombres[1][0]}".ToUpper();
                    return nombres.Length > 0 ? nombres[0].Substring(0, Math.Min(2, nombres[0].Length)).ToUpper() : "XX";
                }
            }
            public int Estrellas => (int)Math.Round(CalificacionPromedio);
            public string ExperienciaTexto => AñosExperiencia == 1 ? "1 año de experiencia" : $"{AñosExperiencia} años de experiencia";
        }

        public class ReviewViewModel
        {
            public int Id { get; set; }
            public string ClienteNombre { get; set; } = string.Empty;
            public string PrestadorNombre { get; set; } = string.Empty;
            public string? Comentario { get; set; }
            public int Calificacion { get; set; }
            public DateTime Fecha { get; set; }
            public string FechaFormateada => Fecha.ToString("dd/MM/yyyy");
            public string TiempoTranscurrido
            {
                get
                {
                    var diferencia = DateTime.Now - Fecha;
                    if (diferencia.TotalDays >= 30)
                        return $"Hace {(int)(diferencia.TotalDays / 30)} meses";
                    else if (diferencia.TotalDays >= 7)
                        return $"Hace {(int)(diferencia.TotalDays / 7)} semanas";
                    else if (diferencia.TotalDays >= 1)
                        return $"Hace {(int)diferencia.TotalDays} días";
                    else if (diferencia.TotalHours >= 1)
                        return $"Hace {(int)diferencia.TotalHours} horas";
                    else
                        return "Hace unos minutos";
                }
            }
            public List<bool> EstrellasArray => Enumerable.Range(1, 5).Select(i => i <= Calificacion).ToList();
        }

        public class StatisticsViewModel
        {
            public int TotalUsuarios { get; set; }
            public int TotalPrestadores { get; set; }
            public int TotalPedidos { get; set; }
            public int TotalMascotas { get; set; }
            public double PromedioCalificaciones { get; set; }
            public string PromedioFormateado => PromedioCalificaciones.ToString("F1");
        }

        public class PetTypeViewModel
        {
            public string Tipo { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public string IconoMascota
            {
                get
                {
                    return Tipo?.ToLower() switch
                    {
                        "perro" => "🐕",
                        "gato" => "🐱",
                        "pez" => "🐠",
                        "ave" or "pájaro" => "🐦",
                        "conejo" => "🐰",
                        "hámster" => "🐹",
                        _ => "🐾"
                    };
                }
            }
        }

        public class LoginViewModel
        {
            [Required(ErrorMessage = "El usuario es requerido")]
            public string Usuario { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es requerida")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public string TipoUsuario { get; set; } = "Dueño"; // Dueño o Prestador

            public bool RecordarMe { get; set; }
        }

        public class RegisterViewModel
        {
            [Required(ErrorMessage = "El usuario es requerido")]
            public string Usuario { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es requerido")]
            [EmailAddress(ErrorMessage = "Formato de correo inválido")]
            public string Correo { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es requerida")]
            [DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirme la contraseña")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
            public string ConfirmarPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "El primer nombre es requerido")]
            public string PrimerNombre { get; set; } = string.Empty;

            public string? SegundoNombre { get; set; }

            [Required(ErrorMessage = "El primer apellido es requerido")]
            public string PrimerApellido { get; set; } = string.Empty;

            public string? SegundoApellido { get; set; }

            [Required(ErrorMessage = "El teléfono es requerido")]
            public string Telefono { get; set; } = string.Empty;

            [Required(ErrorMessage = "El documento de identidad es requerido")]
            public string DocumentoIdentidad { get; set; } = string.Empty;

            [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
            [DataType(DataType.Date)]
            public DateTime FechaNacimiento { get; set; }

            public string TipoUsuario { get; set; } = "Dueño"; // Dueño o Prestador
        }
    }

