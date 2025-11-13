using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbCuentasPrestador
{
    public int IdCuenta { get; set; }

    public int IdPrestador { get; set; }

    public string Banco { get; set; } = null!;

    public string NumeroCuenta { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!;

    public virtual TbPrestadore IdPrestadorNavigation { get; set; } = null!;
}
