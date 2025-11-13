using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbPedido
{
    public int IdPedido { get; set; }

    public int IdUsuarioDueño { get; set; }

    public int IdUsuarioPrestador { get; set; }

    public int IdDireccion { get; set; }

    public DateOnly FechaPedido { get; set; }

    public DateOnly FechaServicio { get; set; }

    public TimeOnly HoraServicioFrom { get; set; }

    public TimeOnly HoraServicioTo { get; set; }

    public int Total { get; set; }

    public string? Estado { get; set; }

    public virtual TbDireccione IdDireccionNavigation { get; set; } = null!;

    public virtual TbUsuario IdUsuarioDueñoNavigation { get; set; } = null!;

    public virtual TbUsuario IdUsuarioPrestadorNavigation { get; set; } = null!;

    public virtual ICollection<TbDetallePedido> TbDetallePedidos { get; set; } = new List<TbDetallePedido>();

    public virtual ICollection<TbPago> TbPagos { get; set; } = new List<TbPago>();

    public virtual ICollection<TbPedidoMascota> TbPedidoMascota { get; set; } = new List<TbPedidoMascota>();

    public virtual ICollection<TbReseña> TbReseñas { get; set; } = new List<TbReseña>();
}
