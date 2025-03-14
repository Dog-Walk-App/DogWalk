using DogWalk_Domain.Common;

namespace DogWalk_Domain.Entities
{
    public class Rol : BaseEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
} 