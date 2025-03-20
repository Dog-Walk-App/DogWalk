using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class ServicioRepository : Repository<Servicio>, IServicioRepository
    {
        public ServicioRepository(DogWalkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Servicio>> GetServiciosByPaseadorIdAsync(int paseadorId)
        {
            return await _context.Precios
                .Where(p => p.PaseadorId == paseadorId)
                .Include(p => p.Servicio)
                .Select(p => p.Servicio)
                .ToListAsync();
        }
    }
} 