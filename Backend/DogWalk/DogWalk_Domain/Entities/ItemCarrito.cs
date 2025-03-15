using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class ItemCarrito : BaseEntity
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int ArticuloId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }

        // Relaciones
        public Carrito Carrito { get; set; } = null!;
        public Articulo Articulo { get; set; } = null!;
    }
} 