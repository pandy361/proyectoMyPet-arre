using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbMascota
{
    public int IdMascota { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string? Raza { get; set; }

    public int? Edad { get; set; }

    public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TbPedidoMascota> TbPedidoMascota { get; set; } = new List<TbPedidoMascota>();
}
