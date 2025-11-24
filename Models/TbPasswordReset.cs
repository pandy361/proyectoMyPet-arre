namespace proyecto_mejoradoMy_pet.Models
{
    public class TbPasswordReset
    {
        public int IdReset { get; set; }
        public int IdUsuario { get; set; }
        public string Token { get; set; } = null!;
        public DateTime? FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }

        public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;
    }
}
