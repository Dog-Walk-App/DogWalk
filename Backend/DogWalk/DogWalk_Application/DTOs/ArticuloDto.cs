namespace DogWalk_Application.DTOs
{
    public class ArticuloDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public class ArticuloCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public class ArticuloUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public class ArticuloWithImagenesDto : ArticuloDto
    {
        public List<ImagenArticuloDto> Imagenes { get; set; } = new List<ImagenArticuloDto>();
    }
} 