using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Perro : BaseEntity
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string? GpsUbicacion { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public ICollection<FotoPerro> Fotos { get; set; } = new List<FotoPerro>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Opinion> Opiniones { get; set; } = new List<Opinion>();
    }
} 