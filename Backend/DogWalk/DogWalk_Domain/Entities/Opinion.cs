using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Opinion : BaseEntity
    {
        public int Id { get; set; }
        public int PerroId { get; set; }
        public int PaseadorId { get; set; }
        public int Puntuacion { get; set; }
        public string? Comentario { get; set; }

        // Relaciones
        public Perro Perro { get; set; } = null!;
        public Paseador Paseador { get; set; } = null!;
    }
} 