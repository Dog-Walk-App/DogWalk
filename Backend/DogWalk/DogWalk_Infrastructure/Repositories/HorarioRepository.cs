using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Domain.Enums;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class HorarioRepository : Repository<Horario>, IHorarioRepository
    {
        public HorarioRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Horario?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(h => h.Paseador)
                .Include(h => h.Reservas)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public override async Task<IEnumerable<Horario>> GetAllAsync()
        {
            return await _dbSet
                .Include(h => h.Paseador)
                .Include(h => h.Reservas)
                .ToListAsync();
        }

        public async Task<IEnumerable<Horario>> GetHorariosByPaseadorIdAsync(int paseadorId)
        {
            return await _dbSet
                .Include(h => h.Reservas)
                .Where(h => h.PaseadorId == paseadorId)
                .OrderBy(h => h.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Horario>> GetHorariosDisponiblesByPaseadorIdAsync(int paseadorId)
        {
            return await _dbSet
                .Include(h => h.Reservas)
                .Where(h => h.PaseadorId == paseadorId && 
                           h.Disponibilidad == DisponibilidadStatus.Disponible &&
                           h.FechaHora > DateTime.Now)
                .OrderBy(h => h.FechaHora)
                .ToListAsync();
        }
    }
} 