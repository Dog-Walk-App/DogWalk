using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IPerroRepository : IRepository<Perro>
    {
        Task<IEnumerable<Perro>> GetPerrosByUsuarioIdAsync(int usuarioId);
        Task<Perro?> GetPerroWithFotosAsync(int perroId);
    }
} 