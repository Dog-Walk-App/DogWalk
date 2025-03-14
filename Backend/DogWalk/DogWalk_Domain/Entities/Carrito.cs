using DogWalk_Domain.Common;
using DogWalk_Domain.Enums;

namespace DogWalk_Domain.Entities
{
    public class Carrito : BaseEntity
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int ArticuloId { get; set; }
        public TipoItem TipoItem { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Articulo Articulo { get; set; } = null!;
    }
} 