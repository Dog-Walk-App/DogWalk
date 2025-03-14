using DogWalk_Domain.Common;
using DogWalk_Domain.Enums;

namespace DogWalk_Domain.Entities
{
    public class Reserva : BaseEntity
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public int ServicioId { get; set; }
        public int PerroId { get; set; }
        public int HorarioId { get; set; }
        public DateTime FechaReserva { get; set; }
        public ReservaStatus Estado { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Paseador Paseador { get; set; } = null!;
        public Servicio Servicio { get; set; } = null!;
        public Perro Perro { get; set; } = null!;
        public Horario Horario { get; set; } = null!;
    }
} 