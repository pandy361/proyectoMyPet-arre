using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbDetallePedido
{
    public int IdDetalle { get; set; }

    public int IdPedido { get; set; }

    public int IdServicio { get; set; }

    public int? Cantidad { get; set; }

    public int Subtotal { get; set; }

    public virtual TbPedido IdPedidoNavigation { get; set; } = null!;

    public virtual TbServicio IdServicioNavigation { get; set; } = null!;
}
