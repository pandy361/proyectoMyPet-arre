using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbPago
{
    public int IdPago { get; set; }

    public int IdPedido { get; set; }

    public int Monto { get; set; }

    public DateTime? FechaPago { get; set; }

    public string Metodo { get; set; } = null!;

    public string? Estado { get; set; }

    public string? IdTransaccionPaypal { get; set; }

    public virtual TbPedido IdPedidoNavigation { get; set; } = null!;
}
