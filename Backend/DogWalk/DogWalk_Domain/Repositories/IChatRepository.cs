using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IChatRepository : IRepository<Chat>
    {
        Task<IEnumerable<Chat>> GetChatByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<Chat>> GetChatByPaseadorIdAsync(int paseadorId);
        Task<IEnumerable<Chat>> GetChatByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId);
    }
} 