using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IOpinionRepository : IRepository<Opinion>
    {
        Task<IEnumerable<Opinion>> GetOpinionesByPerroIdAsync(int perroId);
        Task<IEnumerable<Opinion>> GetOpinionesByPaseadorIdAsync(int paseadorId);
        Task<IEnumerable<Opinion>> GetOpinionesByUsuarioIdAsync(int usuarioId);
        Task<Opinion?> GetOpinionByPerroAndPaseadorAsync(int perroId, int paseadorId);
        Task<Opinion?> GetOpinionByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId);
        Task DeleteAsync(Opinion opinion);
    }
} 