using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbUsuarioImagene
{
    public int IdImagen { get; set; }

    public int IdUsuario { get; set; }

    public string UrlImagen { get; set; } = null!;

    public DateTime? FechaSubida { get; set; }

    public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;
}
