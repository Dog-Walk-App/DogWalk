namespace DogWalk_Application.DTOs
{
    public class FotoPerroDto
    {
        public int Id { get; set; }
        public int PerroId { get; set; }
        public string UrlFoto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class FotoPerroCreateDto
    {
        public int PerroId { get; set; }
        public string UrlFoto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class FotoPerroUpdateDto
    {
        public string UrlFoto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
} 