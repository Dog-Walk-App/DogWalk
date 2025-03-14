using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IRankingRepository : IRepository<Ranking>
    {
        Task<IEnumerable<Ranking>> GetRankingsByPaseadorIdAsync(int paseadorId);
        Task<double> GetValoracionPromedioPaseadorAsync(int paseadorId);
        Task<Ranking?> GetRankingByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId);
    }
} 