using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class CarritoRepository : Repository<Carrito>, ICarritoRepository
    {
        public CarritoRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Carrito?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Usuario)
                .Include(c => c.Articulo)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Carrito>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Usuario)
                .Include(c => c.Articulo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Carrito>> GetCarritoByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(c => c.Articulo)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalCarritoByUsuarioIdAsync(int usuarioId)
        {
            var items = await _dbSet
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            return items.Sum(c => c.PrecioUnitario * c.Cantidad);
        }

        public async Task ClearCarritoByUsuarioIdAsync(int usuarioId)
        {
            var items = await _dbSet
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
} 