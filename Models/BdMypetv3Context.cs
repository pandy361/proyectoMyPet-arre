using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace proyecto_mejoradoMy_pet.Models;

public partial class BdMypetv3Context : DbContext
{
    public BdMypetv3Context()
    {
    }

    public BdMypetv3Context(DbContextOptions<BdMypetv3Context> options)
        : base(options)
    {
    }

    public virtual DbSet<TbCuentasPrestador> TbCuentasPrestadors { get; set; }

    public virtual DbSet<TbDetallePedido> TbDetallePedidos { get; set; }

    public virtual DbSet<TbDireccione> TbDirecciones { get; set; }

    public virtual DbSet<TbDisponibilidad> TbDisponibilidads { get; set; }

    public virtual DbSet<TbMascota> TbMascotas { get; set; }

    public virtual DbSet<TbPago> TbPagos { get; set; }

    public virtual DbSet<TbPedido> TbPedidos { get; set; }

    public virtual DbSet<TbPedidoMascota> TbPedidoMascotas { get; set; }

    public virtual DbSet<TbPrestadore> TbPrestadores { get; set; }

    public virtual DbSet<TbReseña> TbReseñas { get; set; }

    public virtual DbSet<TbRole> TbRoles { get; set; }

    public virtual DbSet<TbServicio> TbServicios { get; set; }

    public virtual DbSet<TbUsuario> TbUsuarios { get; set; }

    public virtual DbSet<TbUsuarioImagene> TbUsuarioImagenes { get; set; }

    public virtual DbSet<TbUsuarioRole> TbUsuarioRoles { get; set; }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-S7IHNNFT\\SQLEXPRESS; database=BdMypetv3; integrated security=true;TrustServerCertificate=True;");
    */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbCuentasPrestador>(entity =>
        {
            entity.HasKey(e => e.IdCuenta).HasName("PK__TbCuenta__C7E2868546D8AEB2");

            entity.ToTable("TbCuentas_Prestador");

            entity.HasIndex(e => e.NumeroCuenta, "UQ__TbCuenta__C6B74B88F1A20A6C").IsUnique();

            entity.Property(e => e.IdCuenta).HasColumnName("id_cuenta");
            entity.Property(e => e.Banco)
                .HasMaxLength(100)
                .HasColumnName("banco");
            entity.Property(e => e.IdPrestador).HasColumnName("id_prestador");
            entity.Property(e => e.NumeroCuenta)
                .HasMaxLength(50)
                .HasColumnName("numero_cuenta");
            entity.Property(e => e.TipoCuenta)
                .HasMaxLength(50)
                .HasColumnName("tipo_cuenta");

            entity.HasOne(d => d.IdPrestadorNavigation).WithMany(p => p.TbCuentasPrestadors)
                .HasForeignKey(d => d.IdPrestador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbCuentas__id_pr__05D8E0BE");
        });

        modelBuilder.Entity<TbDetallePedido>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__TbDetall__4F1332DECF705A44");

            entity.ToTable("TbDetallePedido");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1)
                .HasColumnName("cantidad");
            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.IdServicio).HasColumnName("id_servicio");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TbDetallePedidos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbDetalle__id_pe__6EF57B66");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.TbDetallePedidos)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbDetalle__id_se__6FE99F9F");
        });

        modelBuilder.Entity<TbDireccione>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__TbDirecc__25C35D0717607ADB");

            entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .HasColumnName("departamento");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.EsPredeterminada)
                .HasDefaultValue(false)
                .HasColumnName("es_predeterminada");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .HasColumnName("pais");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TbDirecciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbDirecci__id_us__571DF1D5");
        });

        modelBuilder.Entity<TbDisponibilidad>(entity =>
        {
            entity.HasKey(e => e.IdDisponibilidad).HasName("PK__TbDispon__319B171B4BE59201");

            entity.ToTable("TbDisponibilidad");

            entity.Property(e => e.IdDisponibilidad).HasColumnName("id_disponibilidad");
            entity.Property(e => e.DiaSemana)
                .HasMaxLength(20)
                .HasColumnName("dia_semana");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.IdPrestador).HasColumnName("id_prestador");

            entity.HasOne(d => d.IdPrestadorNavigation).WithMany(p => p.TbDisponibilidads)
                .HasForeignKey(d => d.IdPrestador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbDisponi__id_pr__619B8048");
        });

        modelBuilder.Entity<TbMascota>(entity =>
        {
            entity.HasKey(e => e.IdMascota).HasName("PK__TbMascot__6F037352754B4BED");

            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.Raza)
                .HasMaxLength(50)
                .HasColumnName("raza");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TbMascota)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbMascota__id_us__59FA5E80");
        });

        modelBuilder.Entity<TbPago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__TbPagos__0941B07480B9FFC4");

            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.Metodo)
                .HasMaxLength(50)
                .HasColumnName("metodo");
            entity.Property(e => e.Monto).HasColumnName("monto");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TbPagos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPagos__id_pedi__787EE5A0");
        });

        modelBuilder.Entity<TbPedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PK__TbPedido__6FF01489565859DD");

            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("fecha_pedido");
            entity.Property(e => e.FechaServicio).HasColumnName("fecha_servicio");
            entity.Property(e => e.HoraServicioFrom).HasColumnName("hora_servicio_from");
            entity.Property(e => e.HoraServicioTo).HasColumnName("hora_servicio_to");
            entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
            entity.Property(e => e.IdUsuarioDueño).HasColumnName("id_usuario_dueño");
            entity.Property(e => e.IdUsuarioPrestador).HasColumnName("id_usuario_prestador");
            entity.Property(e => e.Total).HasColumnName("total");

            entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.TbPedidos)
                .HasForeignKey(d => d.IdDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPedidos__id_di__6B24EA82");

            entity.HasOne(d => d.IdUsuarioDueñoNavigation).WithMany(p => p.TbPedidoIdUsuarioDueñoNavigations)
                .HasForeignKey(d => d.IdUsuarioDueño)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPedidos__id_us__693CA210");

            entity.HasOne(d => d.IdUsuarioPrestadorNavigation).WithMany(p => p.TbPedidoIdUsuarioPrestadorNavigations)
                .HasForeignKey(d => d.IdUsuarioPrestador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPedidos__id_us__6A30C649");
        });

        modelBuilder.Entity<TbPedidoMascota>(entity =>
        {
            entity.HasKey(e => e.IdPedidoMascota).HasName("PK__TbPedido__7C1A35AEEEE19F06");

            entity.Property(e => e.IdPedidoMascota).HasColumnName("id_pedido_mascota");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");

            entity.HasOne(d => d.IdMascotaNavigation).WithMany(p => p.TbPedidoMascota)
                .HasForeignKey(d => d.IdMascota)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPedidoM__id_ma__73BA3083");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TbPedidoMascota)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPedidoM__id_pe__72C60C4A");
        });

        modelBuilder.Entity<TbPrestadore>(entity =>
        {
            entity.HasKey(e => e.IdPrestador).HasName("PK__TbPresta__4025E802F56C04DA");

            entity.HasIndex(e => e.IdUsuario, "UQ__TbPresta__4E3E04ACE6A5B5B1").IsUnique();

            entity.Property(e => e.IdPrestador).HasColumnName("id_prestador");
            entity.Property(e => e.AñosExperiencia).HasColumnName("años_experiencia");
            entity.Property(e => e.CalificacionPromedio)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("calificacion_promedio");
            entity.Property(e => e.Experiencia)
                .HasMaxLength(300)
                .HasColumnName("experiencia");
            entity.Property(e => e.Habilidades)
                .HasMaxLength(300)
                .HasColumnName("habilidades");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Resumen)
                .HasMaxLength(300)
                .HasColumnName("resumen");
            entity.Property(e => e.ServiciosOfrecidos)
                .HasMaxLength(300)
                .HasColumnName("servicios_ofrecidos");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.TbPrestadore)
                .HasForeignKey<TbPrestadore>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbPrestad__id_us__5EBF139D");
        });

        modelBuilder.Entity<TbReseña>(entity =>
        {
            entity.HasKey(e => e.IdReseña).HasName("PK__TbReseña__06C98AE0D811C21C");

            entity.Property(e => e.IdReseña).HasColumnName("id_reseña");
            entity.Property(e => e.Calificacion).HasColumnName("calificacion");
            entity.Property(e => e.Comentario)
                .HasMaxLength(300)
                .HasColumnName("comentario");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdPedido).HasColumnName("id_pedido");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.TbReseñas)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbReseñas__id_pe__7D439ABD");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TbReseñas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbReseñas__id_us__7E37BEF6");
        });

        modelBuilder.Entity<TbRole>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__TbRoles__6ABCB5E09EFDB940");

            entity.HasIndex(e => e.Nombre, "UQ__TbRoles__72AFBCC6248A5E70").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TbServicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__TbServic__6FD07FDCB6A92445");

            entity.Property(e => e.IdServicio).HasColumnName("id_servicio");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdPrestador).HasColumnName("id_prestador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio).HasColumnName("precio");

            entity.HasOne(d => d.IdPrestadorNavigation).WithMany(p => p.TbServicios)
                .HasForeignKey(d => d.IdPrestador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbServici__id_pr__6477ECF3");
        });

        modelBuilder.Entity<TbUsuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__TbUsuari__4E3E04ADD1258AB3");

            entity.HasIndex(e => e.DocumentoIdentidad, "UQ__TbUsuari__1A03B13FABB4E858").IsUnique();

            entity.HasIndex(e => e.Correo, "UQ__TbUsuari__2A586E0B8A4231FE").IsUnique();

            entity.HasIndex(e => e.Usuario, "UQ__TbUsuari__9AFF8FC62B3E8A60").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .HasColumnName("correo");
            entity.Property(e => e.DocumentoIdentidad)
                .HasMaxLength(30)
                .HasColumnName("documento_identidad");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .HasColumnName("password");
            entity.Property(e => e.PrimerApellido)
                .HasMaxLength(50)
                .HasColumnName("primer_apellido");
            entity.Property(e => e.PrimerNombre)
                .HasMaxLength(50)
                .HasColumnName("primer_nombre");
            entity.Property(e => e.SegundoApellido)
                .HasMaxLength(50)
                .HasColumnName("segundo_apellido");
            entity.Property(e => e.SegundoNombre)
                .HasMaxLength(50)
                .HasColumnName("segundo_nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.Usuario)
                .HasMaxLength(50)
                .HasColumnName("usuario");
        });

        modelBuilder.Entity<TbUsuarioImagene>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__TbUsuari__27CC268931ACFCC7");

            entity.Property(e => e.IdImagen).HasColumnName("id_imagen");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.UrlImagen)
                .HasMaxLength(200)
                .HasColumnName("url_imagen");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TbUsuarioImagenes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbUsuario__id_us__02084FDA");
        });

        modelBuilder.Entity<TbUsuarioRole>(entity =>
        {
            entity.HasKey(e => e.IdUsuRol).HasName("PK__TbUsuari__033DB92CAF360E08");

            entity.Property(e => e.IdUsuRol).HasColumnName("id_UsuRol");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.TbUsuarioRoles)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbUsuario__id_ro__534D60F1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TbUsuarioRoles)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TbUsuario__id_us__52593CB8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
