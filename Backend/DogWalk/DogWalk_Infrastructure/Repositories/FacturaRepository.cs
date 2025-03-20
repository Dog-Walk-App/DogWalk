using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class FacturaRepository : Repository<Factura>, IFacturaRepository
    {
        public FacturaRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Factura?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Usuario)
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public override async Task<IEnumerable<Factura>> GetAllAsync()
        {
            return await _dbSet
                .Include(f => f.Usuario)
                .Include(f => f.Detalles)
                .ToListAsync();
        }

        public async Task<IEnumerable<Factura>> GetFacturasByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(f => f.Detalles)
                .Where(f => f.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Factura?> GetFacturaWithDetallesAsync(int facturaId)
        {
            return await _dbSet
                .Include(f => f.Usuario)
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == facturaId);
        }

        public async Task<IEnumerable<Factura>> GetFacturasByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _dbSet
                .Include(f => f.Usuario)
                .Include(f => f.Detalles)
                .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin)
                .ToListAsync();
        }
    }
} 