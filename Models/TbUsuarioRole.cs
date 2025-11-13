using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbUsuarioRole
{
    public int IdUsuRol { get; set; }

    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public virtual TbRole IdRolNavigation { get; set; } = null!;

    public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;
}
