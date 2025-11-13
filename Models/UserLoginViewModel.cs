using System.ComponentModel.DataAnnotations;

namespace proyecto_mejoradoMy_pet.Models
{
    public class UserLoginViewModel
    {
        [Required(ErrorMessage = "Seleccione un rol")]
        public string Rol { get; set; } = "Dueño";

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;
    }
}
