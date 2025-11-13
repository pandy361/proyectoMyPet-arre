using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbDisponibilidad
{
    public int IdDisponibilidad { get; set; }

    public int IdPrestador { get; set; }

    public string DiaSemana { get; set; } = null!;

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public virtual TbPrestadore IdPrestadorNavigation { get; set; } = null!;
}
