using System.ComponentModel.DataAnnotations;

namespace proyecto_mejoradoMy_pet.Models
{
    public class EditarInfoPersonalViewModel
    {
        [Required(ErrorMessage = "El primer nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string PrimerNombre { get; set; } = null!;

        [StringLength(50, ErrorMessage = "El segundo nombre no puede exceder 50 caracteres")]
        public string? SegundoNombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string PrimerApellido { get; set; } = null!;

        [StringLength(50, ErrorMessage = "El segundo apellido no puede exceder 50 caracteres")]
        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "El correo es obligatorio para recuperación de contraseña")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string Correo { get; set; } = null!;
    }

    // ViewModel para agregar/editar dirección
    public class Direccion1ViewModel
    {
        public int? IdDireccion { get; set; } // Null si es nueva

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string Direccion { get; set; } = null!;

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string Ciudad { get; set; } = null!;

        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        public string Departamento { get; set; } = null!;

        [Required(ErrorMessage = "El país es obligatorio")]
        [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
        public string Pais { get; set; } = null!;

        public bool EsPredeterminada { get; set; }
    }

    // ViewModel completo para la vista de perfil
    public class PerfilCompletoViewModel
    {
        public EditarInfoPersonalViewModel InfoPersonal { get; set; } = new();
        public List<TbDireccione> Direcciones { get; set; } = new();
        public string NombreCompleto { get; set; } = "";
        public string Usuario { get; set; } = "";
    }
}
