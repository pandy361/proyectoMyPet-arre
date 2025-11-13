using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbPedidoMascota
{
    public int IdPedidoMascota { get; set; }

    public int IdPedido { get; set; }

    public int IdMascota { get; set; }

    public virtual TbMascota IdMascotaNavigation { get; set; } = null!;

    public virtual TbPedido IdPedidoNavigation { get; set; } = null!;
}
