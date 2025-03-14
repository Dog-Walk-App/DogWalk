namespace DogWalk_Application.DTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombrePaseador { get; set; } = string.Empty;
    }

    public class ChatCreateDto
    {
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class ChatMessageDto
    {
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public bool IsFromUsuario { get; set; }
    }
} 