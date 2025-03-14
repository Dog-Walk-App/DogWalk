using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class DetalleFacturaDto
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public TipoItem TipoItem { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string TipoItemNombre => TipoItem.ToString();
        public string NombreItem { get; set; } = string.Empty;
    }

    public class DetalleFacturaCreateDto
    {
        public int FacturaId { get; set; }
        public TipoItem TipoItem { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
} 