using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class FacturaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal Total { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string MetodoPagoNombre => MetodoPago.ToString();
        public string NombreUsuario { get; set; } = string.Empty;
    }

    public class FacturaCreateDto
    {
        public int UsuarioId { get; set; }
        public MetodoPago MetodoPago { get; set; }
    }

    public class FacturaDetailDto : FacturaDto
    {
        public List<DetalleFacturaDto> Detalles { get; set; } = new List<DetalleFacturaDto>();
        public UsuarioDto Usuario { get; set; } = null!;
    }
} 