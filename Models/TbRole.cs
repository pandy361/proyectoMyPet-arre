using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbRole
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<TbUsuarioRole> TbUsuarioRoles { get; set; } = new List<TbUsuarioRole>();
}
