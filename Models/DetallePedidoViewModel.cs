namespace proyecto_mejoradoMy_pet.Models
{
    public class DetallePedidoViewModel
    {
        // INFORMACIÓN DEL PEDIDO
        public int IdPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateOnly FechaPedido { get; set; }
        public DateOnly FechaServicio { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int Total { get; set; }

        // INFORMACIÓN DEL PRESTADOR
        public int IdPrestador { get; set; }
        public string NombrePrestador { get; set; } = string.Empty;
        public string TelefonoPrestador { get; set; } = string.Empty;
        public string EmailPrestador { get; set; } = string.Empty;
        public decimal CalificacionPrestador { get; set; }
        public string? FotoPrestador { get; set; }
        public string? DescripcionPrestador { get; set; }

        // INFORMACIÓN DEL DUEÑO
        public string NombreDueño { get; set; } = string.Empty;
        public string TelefonoDueño { get; set; } = string.Empty;
        public string EmailDueño { get; set; } = string.Empty;

        // DIRECCIÓN
        public string DireccionCompleta { get; set; } = string.Empty;

        // SERVICIOS
        public List<ServicioDetalle> Servicios { get; set; } = new();

        // MASCOTAS
        public List<MascotaDetalle> Mascotas { get; set; } = new();

        // RESEÑA (si existe)
        public ReseñaDetalle? Reseña { get; set; }

        // CONTROL DE RESEÑAS
        public bool PuedeDejarReseña { get; set; } // Solo si está "Completado"
        public bool YaDejoReseña { get; set; }
    }

    public class ServicioDetalle
    {
        public string Nombre { get; set; } = string.Empty;
        public int Precio { get; set; }
        public int Cantidad { get; set; }
        public int Subtotal { get; set; }
    }

    public class MascotaDetalle
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Raza { get; set; }
    }

    public class ReseñaDetalle
    {
        public int IdReseña { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }

    // REQUEST PARA ENVIAR RESEÑA
    public class CrearReseñaRequest
    {
        public int IdPedido { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
    }
}
