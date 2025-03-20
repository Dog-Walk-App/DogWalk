using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class ArticuloRepository : Repository<Articulo>, IArticuloRepository
    {
        public ArticuloRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Articulo?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Imagenes)
                .Include(a => a.CarritoItems)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public override async Task<IEnumerable<Articulo>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.Imagenes)
                .Include(a => a.CarritoItems)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articulo>> GetArticulosWithImagenesAsync()
        {
            return await _dbSet
                .Include(a => a.Imagenes)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articulo>> GetArticulosByStockMinimoAsync(int stockMinimo)
        {
            return await _dbSet
                .Where(a => a.Stock <= stockMinimo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articulo>> GetArticulosByCategoriaAsync(string categoria)
        {
            return await _dbSet
                .Where(a => a.Categoria == categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<ImagenArticulo>> GetImagenesArticuloAsync(int articuloId)
        {
            var articulo = await _dbSet
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.Id == articuloId);

            return articulo?.Imagenes ?? new List<ImagenArticulo>();
        }

        public async Task<bool> IsArticuloInCarritoAsync(int articuloId)
        {
            return await _dbSet
                .Include(a => a.CarritoItems)
                .AnyAsync(a => a.Id == articuloId && a.CarritoItems.Any());
        }

        public async Task AddImagenArticuloAsync(ImagenArticulo imagen)
        {
            var articulo = await _dbSet
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.Id == imagen.ArticuloId);

            if (articulo != null)
            {
                articulo.Imagenes.Add(imagen);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteImagenesArticuloAsync(int articuloId)
        {
            var articulo = await _dbSet
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.Id == articuloId);

            if (articulo != null)
            {
                articulo.Imagenes.Clear();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ImagenArticulo?> GetImagenArticuloByIdAsync(int imagenId)
        {
            return await _context.Set<ImagenArticulo>()
                .FirstOrDefaultAsync(i => i.Id == imagenId);
        }

        public async Task DeleteImagenArticuloAsync(ImagenArticulo imagen)
        {
            _context.Set<ImagenArticulo>().Remove(imagen);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Articulo>> GetArticulosByNombreAsync(string nombre)
        {
            return await _dbSet
                .Include(a => a.Imagenes)
                .Where(a => a.Nombre.Contains(nombre))
                .ToListAsync();
        }

        public async Task<IEnumerable<Articulo>> GetArticulosByPrecioRangoAsync(decimal precioMin, decimal precioMax)
        {
            return await _dbSet
                .Include(a => a.Imagenes)
                .Where(a => a.Precio >= precioMin && a.Precio <= precioMax)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articulo>> GetArticulosEnStockAsync()
        {
            return await _dbSet
                .Include(a => a.Imagenes)
                .Where(a => a.Stock > 0)
                .ToListAsync();
        }
    }
} 