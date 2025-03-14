using DogWalk_Domain.Common;
using DogWalk_Domain.Enums;

namespace DogWalk_Domain.Entities
{
    public class DetalleFactura : BaseEntity
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public TipoItem TipoItem { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Relaciones
        public Factura Factura { get; set; } = null!;
    }
} 