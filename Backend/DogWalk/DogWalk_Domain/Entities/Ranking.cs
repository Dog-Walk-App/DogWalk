using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Ranking : BaseEntity
    {
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string? Comentario { get; set; }
        public int Valoracion { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Paseador Paseador { get; set; } = null!;
    }
} 