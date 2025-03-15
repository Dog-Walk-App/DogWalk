using System;

namespace DogWalk_Application.DTOs
{
    public class OpinionDto
    {
        public int Id { get; set; }
        public int PaseadorId { get; set; }
        public int UsuarioId { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public int Valoracion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombrePaseador { get; set; } = string.Empty;
    }

    public class OpinionCreateDto
    {
        public int PaseadorId { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public int Valoracion { get; set; }
    }

    public class OpinionUpdateDto
    {
        public string Comentario { get; set; } = string.Empty;
        public int Valoracion { get; set; }
    }
} 