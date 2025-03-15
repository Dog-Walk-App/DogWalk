using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Articulo : BaseEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string? ImagenPrincipal { get; set; }

        // Relaciones
        public ICollection<ImagenArticulo> Imagenes { get; set; } = new List<ImagenArticulo>();
        public ICollection<Carrito> CarritoItems { get; set; } = new List<Carrito>();
        public ICollection<DetalleFactura> DetallesFactura { get; set; } = new List<DetalleFactura>();
    }
} 