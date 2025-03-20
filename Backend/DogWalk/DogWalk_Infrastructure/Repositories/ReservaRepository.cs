using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Domain.Enums;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class ReservaRepository : Repository<Reserva>, IReservaRepository
    {
        public ReservaRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Reserva?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<Reserva>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasByPaseadorIdAsync(int paseadorId)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .Where(r => r.PaseadorId == paseadorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasByPerroIdAsync(int perroId)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Horario)
                .Where(r => r.PerroId == perroId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasByEstadoAsync(ReservaStatus estado)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .Where(r => r.Estado == estado)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasByFechaAsync(DateTime fecha)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .Where(r => r.Horario.FechaHora.Date == fecha.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasCompletadasByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId)
        {
            return await _dbSet
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Horario)
                .Where(r => r.UsuarioId == usuarioId && 
                           r.PaseadorId == paseadorId && 
                           r.Estado == ReservaStatus.Confirmada)
                .ToListAsync();
        }
    }
} 