using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IServicioRepository : IRepository<Servicio>
    {
        Task<IEnumerable<Servicio>> GetServiciosByPaseadorIdAsync(int paseadorId);
    }
} 