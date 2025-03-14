using DogWalk_Domain.Common;
using DogWalk_Domain.ValueObjects;

namespace DogWalk_Domain.Entities
{
    public class Usuario : BaseEntity
    {
        public int Id { get; set; }
        public int RolId { get; set; }
        public Dni Dni { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public Email Email { get; set; } = null!;
        public string Password { get; set; } = string.Empty;
        public Telefono Telefono { get; set; } = null!;
        public string? FotoPerfil { get; set; }

        // Relaciones
        public Rol Rol { get; set; } = null!;
        public ICollection<Perro> Perros { get; set; } = new List<Perro>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Carrito> CarritoItems { get; set; } = new List<Carrito>();
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();
    }
} 