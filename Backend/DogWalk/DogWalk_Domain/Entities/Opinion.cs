using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Opinion : BaseEntity
    {
        public int Id { get; set; }
        public int PerroId { get; set; }
        public int PaseadorId { get; set; }
        public int UsuarioId { get; set; }
        public int Valoracion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Relaciones
        public Perro Perro { get; set; } = null!;
        public Paseador Paseador { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
} 