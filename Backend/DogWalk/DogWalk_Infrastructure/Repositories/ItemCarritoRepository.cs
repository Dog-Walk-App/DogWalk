using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class ItemCarritoRepository : Repository<ItemCarrito>, IItemCarritoRepository
    {
        public ItemCarritoRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<ItemCarrito?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Carrito)
                .Include(i => i.Articulo)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public override async Task<IEnumerable<ItemCarrito>> GetAllAsync()
        {
            return await _dbSet
                .Include(i => i.Carrito)
                .Include(i => i.Articulo)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemCarrito>> GetItemsCarritoAsync(int carritoId)
        {
            return await _dbSet
                .Include(i => i.Articulo)
                .Where(i => i.CarritoId == carritoId)
                .ToListAsync();
        }

        public async Task<ItemCarrito?> GetItemCarritoByArticuloIdAsync(int carritoId, int articuloId)
        {
            return await _dbSet
                .Include(i => i.Articulo)
                .FirstOrDefaultAsync(i => i.CarritoId == carritoId && i.ArticuloId == articuloId);
        }

        public async Task<ItemCarrito?> GetItemCarritoByIdAsync(int itemId)
        {
            return await GetByIdAsync(itemId);
        }

        public async Task AddItemCarritoAsync(ItemCarrito item)
        {
            await _dbSet.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemCarritoAsync(ItemCarrito item)
        {
            _dbSet.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCarritoAsync(int carritoId)
        {
            var items = await _dbSet
                .Where(i => i.CarritoId == carritoId)
                .ToListAsync();

            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
} 