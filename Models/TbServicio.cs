using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbServicio
{
    public int IdServicio { get; set; }

    public int IdPrestador { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int Precio { get; set; }

    public virtual TbPrestadore IdPrestadorNavigation { get; set; } = null!;

    public virtual ICollection<TbDetallePedido> TbDetallePedidos { get; set; } = new List<TbDetallePedido>();
}
