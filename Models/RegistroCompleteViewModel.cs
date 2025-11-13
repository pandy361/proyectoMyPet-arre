using System.ComponentModel.DataAnnotations;

namespace proyecto_mejoradoMy_pet.Models
{
    public class RegistroCompleteViewModel
    {
        // Información personal
        [Required(ErrorMessage = "El usuario es requerido")]
        public string Usuario { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirme la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "El primer nombre es requerido")]
        public string PrimerNombre { get; set; } = null!;

        public string? SegundoNombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es requerido")]
        public string PrimerApellido { get; set; } = null!;

        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "El documento es requerido")]
        public string Documento { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public string FechaNacimiento { get; set; } = null!; // Cambiado a string

        // Roles seleccionados
        [Required(ErrorMessage = "Debe seleccionar al menos un rol")]
        public List<string> RolesSeleccionados { get; set; } = new List<string>();

        // Direcciones
        public List<DireccionViewModel> Direcciones { get; set; } = new List<DireccionViewModel>();

        // Información de prestador (opcional)
        public PrestadorInfoViewModel? InfoPrestador { get; set; }

        // Mascotas (opcional)
        public List<MascotaViewModel> Mascotas { get; set; } = new List<MascotaViewModel>();
    }

    public class DireccionViewModel
    {
        [Required(ErrorMessage = "La dirección es requerida")]
        public string Direccion { get; set; } = null!;

        [Required(ErrorMessage = "La ciudad es requerida")]
        public string Ciudad { get; set; } = null!;

        [Required(ErrorMessage = "El departamento es requerido")]
        public string Departamento { get; set; } = null!;

        [Required(ErrorMessage = "El país es requerido")]
        public string Pais { get; set; } = null!;

        public bool EsPredeterminada { get; set; }
    }

    public class PrestadorInfoViewModel
    {
        [Required(ErrorMessage = "El resumen es requerido")]
        public string Resumen { get; set; } = null!;

        [Required(ErrorMessage = "Las habilidades son requeridas")]
        public string Habilidades { get; set; } = null!;

        public string? Experiencia { get; set; }

        [Required(ErrorMessage = "Los años de experiencia son requeridos")]
        public int AnosExperiencia { get; set; }

        public List<ServicioViewModel> Servicios { get; set; } = new List<ServicioViewModel>();
        public List<DisponibilidadViewModel> Disponibilidad { get; set; } = new List<DisponibilidadViewModel>();
    }

    public class ServicioViewModel
    {
        [Required(ErrorMessage = "El nombre del servicio es requerido")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public int Precio { get; set; }
    }

    public class DisponibilidadViewModel
    {
        [Required(ErrorMessage = "El día es requerido")]
        public string DiaSemana { get; set; } = null!;

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        public string HoraInicio { get; set; } = null!; // Cambiado a string

        [Required(ErrorMessage = "La hora de fin es requerida")]
        public string HoraFin { get; set; } = null!; // Cambiado a string
    }

    public class MascotaViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El tipo es requerido")]
        public string Tipo { get; set; } = null!;

        public string? Raza { get; set; }
        public int? Edad { get; set; }
    }
}