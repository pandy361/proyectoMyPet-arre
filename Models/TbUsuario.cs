using System;
using System.Collections.Generic;

namespace proyecto_mejoradoMy_pet.Models;

public partial class TbUsuario
{
    public int IdUsuario { get; set; }

    public string Usuario { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string PrimerNombre { get; set; } = null!;

    public string? SegundoNombre { get; set; }

    public string PrimerApellido { get; set; } = null!;

    public string? SegundoApellido { get; set; }

    public string Telefono { get; set; } = null!;

    public string DocumentoIdentidad { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<TbDireccione> TbDirecciones { get; set; } = new List<TbDireccione>();

    public virtual ICollection<TbMascota> TbMascota { get; set; } = new List<TbMascota>();

    public virtual ICollection<TbPedido> TbPedidoIdUsuarioDueñoNavigations { get; set; } = new List<TbPedido>();

    public virtual ICollection<TbPedido> TbPedidoIdUsuarioPrestadorNavigations { get; set; } = new List<TbPedido>();

    public virtual TbPrestadore? TbPrestadore { get; set; }

    public virtual ICollection<TbReseña> TbReseñas { get; set; } = new List<TbReseña>();

    public virtual ICollection<TbUsuarioImagene> TbUsuarioImagenes { get; set; } = new List<TbUsuarioImagene>();

    public virtual ICollection<TbUsuarioRole> TbUsuarioRoles { get; set; } = new List<TbUsuarioRole>();
}
