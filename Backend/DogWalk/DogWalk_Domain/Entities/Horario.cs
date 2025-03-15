using DogWalk_Domain.Common;
using DogWalk_Domain.Enums;

namespace DogWalk_Domain.Entities
{
    public class Horario : BaseEntity
    {
        public int Id { get; set; }
        public int PaseadorId { get; set; }
        public DateTime FechaHora { get; set; }
        public DisponibilidadStatus Disponibilidad { get; set; }

        // Relaciones
        public Paseador Paseador { get; set; } = null!;
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
} 