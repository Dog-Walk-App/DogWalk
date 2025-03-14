using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class ImagenArticulo : BaseEntity
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public string UrlImagen { get; set; } = string.Empty;

        // Relaciones
        public Articulo Articulo { get; set; } = null!;
    }
} 