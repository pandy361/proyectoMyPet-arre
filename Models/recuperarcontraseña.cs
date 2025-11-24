using System.ComponentModel.DataAnnotations;

namespace proyecto_mejoradoMy_pet.Models
{
    // ViewModel para solicitar recuperación
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = null!;
    }

    // ViewModel para restablecer contraseña
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirme la contraseña")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; } = null!;
    }
    
}
