using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Servicio : BaseEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Relaciones
        public ICollection<Precio> Precios { get; set; } = new List<Precio>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
} 