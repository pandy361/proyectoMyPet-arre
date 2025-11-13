using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbPrestadore
{
    public int IdPrestador { get; set; }

    public int IdUsuario { get; set; }

    public string? Resumen { get; set; }

    public string? Habilidades { get; set; }

    public string? ServiciosOfrecidos { get; set; }

    public string? Experiencia { get; set; }

    public int? AñosExperiencia { get; set; }

    public decimal? CalificacionPromedio { get; set; }

    public virtual TbUsuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TbCuentasPrestador> TbCuentasPrestadors { get; set; } = new List<TbCuentasPrestador>();

    public virtual ICollection<TbDisponibilidad> TbDisponibilidads { get; set; } = new List<TbDisponibilidad>();

    public virtual ICollection<TbServicio> TbServicios { get; set; } = new List<TbServicio>();
}
