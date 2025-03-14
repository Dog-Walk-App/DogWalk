namespace DogWalk_Application.DTOs
{
    public class RankingDto
    {
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string? Comentario { get; set; }
        public int Valoracion { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombrePaseador { get; set; } = string.Empty;
    }

    public class RankingCreateDto
    {
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string? Comentario { get; set; }
        public int Valoracion { get; set; }
    }

    public class RankingUpdateDto
    {
        public string? Comentario { get; set; }
        public int Valoracion { get; set; }
    }
} 