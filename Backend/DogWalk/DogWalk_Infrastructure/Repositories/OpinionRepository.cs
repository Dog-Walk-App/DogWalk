using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class OpinionRepository : Repository<Opinion>, IOpinionRepository
    {
        public OpinionRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Opinion?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(o => o.Usuario)
                .Include(o => o.Paseador)
                .Include(o => o.Perro)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public override async Task<IEnumerable<Opinion>> GetAllAsync()
        {
            return await _dbSet
                .Include(o => o.Usuario)
                .Include(o => o.Paseador)
                .Include(o => o.Perro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Opinion>> GetOpinionesByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(o => o.Paseador)
                .Include(o => o.Perro)
                .Where(o => o.UsuarioId == usuarioId)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Opinion>> GetOpinionesByPerroIdAsync(int perroId)
        {
            return await _dbSet
                .Include(o => o.Usuario)
                .Include(o => o.Paseador)
                .Where(o => o.PerroId == perroId)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Opinion>> GetOpinionesByPaseadorIdAsync(int paseadorId)
        {
            return await _dbSet
                .Include(o => o.Usuario)
                .Include(o => o.Perro)
                .Where(o => o.PaseadorId == paseadorId)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Opinion?> GetOpinionByPerroAndPaseadorAsync(int perroId, int paseadorId)
        {
            return await _dbSet
                .Include(o => o.Usuario)
                .FirstOrDefaultAsync(o => o.PerroId == perroId && o.PaseadorId == paseadorId);
        }

        public async Task<Opinion?> GetOpinionByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId)
        {
            return await _dbSet
                .Include(o => o.Perro)
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId && o.PaseadorId == paseadorId);
        }
    }
} 