using DogWalk_Domain.Common;
using DogWalk_Domain.Enums;

namespace DogWalk_Domain.Entities
{
    public class Factura : BaseEntity
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal Total { get; set; }
        public MetodoPago MetodoPago { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
} 