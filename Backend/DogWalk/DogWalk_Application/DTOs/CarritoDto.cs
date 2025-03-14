using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int ArticuloId { get; set; }
        public TipoItem TipoItem { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreArticulo { get; set; } = string.Empty;
        public string TipoItemNombre => TipoItem.ToString();
    }

    public class CarritoCreateDto
    {
        public int UsuarioId { get; set; }
        public int ArticuloId { get; set; }
        public TipoItem TipoItem { get; set; }
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CarritoUpdateDto
    {
        public int Cantidad { get; set; }
    }

    public class CarritoDetailDto : CarritoDto
    {
        public UsuarioDto Usuario { get; set; } = null!;
        public ArticuloDto Articulo { get; set; } = null!;
    }
} 