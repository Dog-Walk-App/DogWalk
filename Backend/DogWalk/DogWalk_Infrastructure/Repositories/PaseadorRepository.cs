using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class PaseadorRepository : Repository<Paseador>, IPaseadorRepository
    {
        public PaseadorRepository(DogWalkDbContext context) : base(context)
        {
        }

        public async Task<Paseador?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Email.Value == email);
        }

        public async Task<IEnumerable<Paseador>> GetPaseadoresByValoracionAsync(decimal valoracionMinima)
        {
            return await _dbSet
                .Include(p => p.Rankings)
                .Where(p => p.Rankings.Average(r => (decimal)r.Valoracion) >= valoracionMinima)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paseador>> GetPaseadoresCercaDeUbicacionAsync(double latitud, double longitud, double distanciaMaximaKm)
        {
            // Implementar lógica de cálculo de distancia
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Servicio>> GetServiciosByPaseadorIdAsync(int paseadorId)
        {
            var paseador = await _dbSet
                .Include(p => p.Precios)
                .ThenInclude(p => p.Servicio)
                .FirstOrDefaultAsync(p => p.Id == paseadorId);

            return paseador?.Precios.Select(p => p.Servicio) ?? new List<Servicio>();
        }

        public async Task<decimal> GetPrecioServicioAsync(int paseadorId, int servicioId)
        {
            var precio = await _context.Precios
                .FirstOrDefaultAsync(p => p.PaseadorId == paseadorId && p.ServicioId == servicioId);

            return precio?.Valor ?? 0;
        }
    }
} 