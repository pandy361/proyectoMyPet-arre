using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbReseña
{
    public int IdReseña { get; set; }

    public int IdPedido { get; set; }

    public int IdUsuario { get; set; }

    public string? Comentario { get; set; }

    public int? Calificacion { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual TbPedido IdPedidoNavigation { get; set; } = null!;

    public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;
}
