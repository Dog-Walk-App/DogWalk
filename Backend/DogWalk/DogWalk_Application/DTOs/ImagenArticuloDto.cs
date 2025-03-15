namespace DogWalk_Application.DTOs
{
    public class ImagenArticuloDto
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public string UrlImagen { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ImagenArticuloCreateDto
    {
        public int ArticuloId { get; set; }
        public string UrlImagen { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ImagenArticuloUpdateDto
    {
        public string UrlImagen { get; set; } = string.Empty;
    }
} 