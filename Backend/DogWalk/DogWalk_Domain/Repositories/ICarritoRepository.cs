using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface ICarritoRepository : IRepository<Carrito>
    {
        Task<IEnumerable<Carrito>> GetCarritoByUsuarioIdAsync(int usuarioId);
        Task<decimal> GetTotalCarritoByUsuarioIdAsync(int usuarioId);
        Task ClearCarritoByUsuarioIdAsync(int usuarioId);
    }
} 