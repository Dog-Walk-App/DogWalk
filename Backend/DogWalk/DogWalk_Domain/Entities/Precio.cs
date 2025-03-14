using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Precio : BaseEntity
    {
        public int PaseadorId { get; set; }
        public int ServicioId { get; set; }
        public decimal Valor { get; set; }

        // Relaciones
        public Paseador Paseador { get; set; } = null!;
        public Servicio Servicio { get; set; } = null!;
    }
} 