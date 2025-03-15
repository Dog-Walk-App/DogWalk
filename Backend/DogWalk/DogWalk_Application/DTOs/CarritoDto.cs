using System;
using System.Collections.Generic;
using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<ItemCarritoDto> Items { get; set; } = new List<ItemCarritoDto>();
        public decimal Total { get; set; }
    }

    public class ItemCarritoDto
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string NombreArticulo { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
    }

    public class ItemCarritoCreateDto
    {
        public int ArticuloId { get; set; }
        public int Cantidad { get; set; }
    }

    public class ItemCarritoUpdateDto
    {
        public int Cantidad { get; set; }
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