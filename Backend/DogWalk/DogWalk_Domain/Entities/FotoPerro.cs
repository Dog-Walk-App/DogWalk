using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class FotoPerro : BaseEntity
    {
        public int Id { get; set; }
        public int PerroId { get; set; }
        public string UrlFoto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // Relaciones
        public Perro Perro { get; set; } = null!;
    }
} 