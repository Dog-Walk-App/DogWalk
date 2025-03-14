using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Chat : BaseEntity
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PaseadorId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Paseador Paseador { get; set; } = null!;
    }
} 