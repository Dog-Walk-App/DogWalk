using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<IEnumerable<Usuario>> GetUsuariosWithPerrosAsync();
    }
} 