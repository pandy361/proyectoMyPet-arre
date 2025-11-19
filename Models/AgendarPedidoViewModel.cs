namespace proyecto_mejoradoMy_pet.Models
{
    public class AgendarPedidoViewModel
    {
        public int IdPrestador { get; set; }
        public string NombrePrestador { get; set; } = string.Empty;
        public decimal CalificacionPrestador { get; set; }
        public List<ServicioSeleccionViewModel> ServiciosDisponibles { get; set; } = new();
        public List<DisponibilidadViewModel> Disponibilidad { get; set; } = new();
        public List<MascotaSeleccionViewModel> MascotasUsuario { get; set; } = new();
        public List<DireccionSeleccionViewModel> DireccionesUsuario { get; set; } = new();
    }

    public class ServicioSeleccionViewModel
    {
        public int IdServicio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Precio { get; set; }
        public bool Seleccionado { get; set; }
    }

    public class Disponibilidad1ViewModel
    {
        public string DiaSemana { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
    }

    public class MascotaSeleccionViewModel
    {
        public int IdMascota { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public bool Seleccionada { get; set; }
    }

    public class DireccionSeleccionViewModel
    {
        public int IdDireccion { get; set; }
        public string DireccionCompleta { get; set; } = string.Empty;
        public bool EsPredeterminada { get; set; }
    }

    public class ProcesarPedidoRequest
    {
        public int IdPrestador { get; set; }
        public List<int> ServiciosSeleccionados { get; set; } = new();
        public List<int> MascotasSeleccionadas { get; set; } = new();
        public int IdDireccion { get; set; }
        public string FechaServicio { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;

        public string? PaypalOrderId { get; set; }
    }
}