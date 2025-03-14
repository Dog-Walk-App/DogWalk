using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IArticuloRepository : IRepository<Articulo>
    {
        Task<IEnumerable<Articulo>> GetArticulosWithImagenesAsync();
        Task<IEnumerable<Articulo>> GetArticulosByStockMinimoAsync(int stockMinimo);
    }
} 