using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class RankingRepository : Repository<Ranking>, IRankingRepository
    {
        public RankingRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Ranking?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .FirstOrDefaultAsync(r => r.UsuarioId == id);
        }

        public override async Task<IEnumerable<Ranking>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ranking>> GetRankingsByPaseadorIdAsync(int paseadorId)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Where(r => r.PaseadorId == paseadorId)
                .OrderByDescending(r => r.Valoracion)
                .ToListAsync();
        }

        public async Task<double> GetValoracionPromedioPaseadorAsync(int paseadorId)
        {
            var rankings = await _dbSet
                .Where(r => r.PaseadorId == paseadorId)
                .ToListAsync();

            if (!rankings.Any())
                return 0;

            return rankings.Average(r => r.Valoracion);
        }

        public async Task<Ranking?> GetRankingByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.PaseadorId == paseadorId);
        }
    }
}