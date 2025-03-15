using DogWalk_Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogWalk_Domain.Repositories
{
    public interface IItemCarritoRepository : IRepository<ItemCarrito>
    {
        Task<IEnumerable<ItemCarrito>> GetItemsCarritoAsync(int carritoId);
        Task<ItemCarrito?> GetItemCarritoByArticuloIdAsync(int carritoId, int articuloId);
        Task<ItemCarrito?> GetItemCarritoByIdAsync(int itemId);
        Task AddItemCarritoAsync(ItemCarrito item);
        Task DeleteItemCarritoAsync(ItemCarrito item);
        Task ClearCarritoAsync(int carritoId);
    }
} 