using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class PerroRepository : Repository<Perro>, IPerroRepository
    {
        public PerroRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Perro?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Fotos)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Perro>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Fotos)
                .ToListAsync();
        }

        public async Task<IEnumerable<Perro>> GetPerrosByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(p => p.Fotos)
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Perro?> GetPerroWithFotosAsync(int perroId)
        {
            return await _dbSet
                .Include(p => p.Fotos)
                .FirstOrDefaultAsync(p => p.Id == perroId);
        }
    }
} 