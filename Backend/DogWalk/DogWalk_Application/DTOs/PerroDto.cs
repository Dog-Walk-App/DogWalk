namespace DogWalk_Application.DTOs
{
    public class PerroDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string? GpsUbicacion { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
    }

    public class PerroCreateDto
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string? GpsUbicacion { get; set; }
    }

    public class PerroUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string? GpsUbicacion { get; set; }
    }

    public class PerroWithFotosDto : PerroDto
    {
        public List<FotoPerroDto> Fotos { get; set; } = new List<FotoPerroDto>();
    }
} 