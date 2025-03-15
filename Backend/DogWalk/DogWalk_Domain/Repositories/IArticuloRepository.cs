using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IArticuloRepository : IRepository<Articulo>
    {
        Task<IEnumerable<Articulo>> GetArticulosWithImagenesAsync();
        Task<IEnumerable<Articulo>> GetArticulosByStockMinimoAsync(int stockMinimo);
        Task<IEnumerable<Articulo>> GetArticulosByCategoriaAsync(string categoria);
        Task<IEnumerable<ImagenArticulo>> GetImagenesArticuloAsync(int articuloId);
        Task<bool> IsArticuloInCarritoAsync(int articuloId);
        Task AddImagenArticuloAsync(ImagenArticulo imagen);
        Task DeleteImagenesArticuloAsync(int articuloId);
        Task<ImagenArticulo?> GetImagenArticuloByIdAsync(int imagenId);
        Task DeleteImagenArticuloAsync(ImagenArticulo imagen);
    }
} 