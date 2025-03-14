namespace DogWalk_Application.DTOs
{
    public class OpinionDto
    {
        public int Id { get; set; }
        public int PerroId { get; set; }
        public int PaseadorId { get; set; }
        public int Puntuacion { get; set; }
        public string? Comentario { get; set; }
        public string NombrePerro { get; set; } = string.Empty;
        public string NombrePaseador { get; set; } = string.Empty;
    }

    public class OpinionCreateDto
    {
        public int PerroId { get; set; }
        public int PaseadorId { get; set; }
        public int Puntuacion { get; set; }
        public string? Comentario { get; set; }
    }

    public class OpinionUpdateDto
    {
        public int Puntuacion { get; set; }
        public string? Comentario { get; set; }
    }
} 