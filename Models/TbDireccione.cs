using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbDireccione
{
    public int IdDireccion { get; set; }

    public int IdUsuario { get; set; }

    public string Direccion { get; set; } = null!;

    public string Ciudad { get; set; } = null!;

    public string Departamento { get; set; } = null!;

    public string Pais { get; set; } = null!;

    public bool? EsPredeterminada { get; set; }

    public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TbPedido> TbPedidos { get; set; } = new List<TbPedido>();
}
