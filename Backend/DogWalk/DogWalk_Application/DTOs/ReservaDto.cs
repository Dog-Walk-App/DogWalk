using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class ReservaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public int ServicioId { get; set; }
        public int PerroId { get; set; }
        public int HorarioId { get; set; }
        public DateTime FechaReserva { get; set; }
        public ReservaStatus Estado { get; set; }
        public string EstadoNombre => Estado.ToString();
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombrePaseador { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public string NombrePerro { get; set; } = string.Empty;
        public DateTime FechaHoraServicio { get; set; }
    }

    public class ReservaCreateDto
    {
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public int ServicioId { get; set; }
        public int PerroId { get; set; }
        public int HorarioId { get; set; }
        public DateTime FechaReserva { get; set; }
    }

    public class ReservaUpdateDto
    {
        public ReservaStatus Estado { get; set; }
    }

    public class ReservaDetailDto : ReservaDto
    {
        public UsuarioDto Usuario { get; set; } = null!;
        public PaseadorDto Paseador { get; set; } = null!;
        public ServicioDto Servicio { get; set; } = null!;
        public PerroDto Perro { get; set; } = null!;
        public HorarioDto Horario { get; set; } = null!;
    }
} 