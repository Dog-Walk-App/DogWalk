using DogWalk_Domain.Common;
using DogWalk_Domain.ValueObjects;

namespace DogWalk_Domain.Entities
{
    public class Paseador : BaseEntity
    {
        public int Id { get; set; }
        public Dni Dni { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public Email Email { get; set; } = null!;
        public string Password { get; set; } = string.Empty;
        public Telefono Telefono { get; set; } = null!;
        public double ValoracionMedia { get; set; }
        public int ValoracionGeneral { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public Coordenadas Ubicacion { get; set; } = null!;

        // Relaciones
        public ICollection<Precio> Precios { get; set; } = new List<Precio>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();
        public ICollection<Opinion> Opiniones { get; set; } = new List<Opinion>();
        public ICollection<Horario> Horarios { get; set; } = new List<Horario>();
    }
} 