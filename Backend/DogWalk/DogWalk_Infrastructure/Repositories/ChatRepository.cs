using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class ChatRepository : Repository<Chat>, IChatRepository
    {
        public ChatRepository(DogWalkDbContext context) : base(context)
        {
        }

        public override async Task<Chat?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Usuario)
                .Include(c => c.Paseador)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Chat>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Usuario)
                .Include(c => c.Paseador)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetChatByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(c => c.Paseador)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetChatByPaseadorIdAsync(int paseadorId)
        {
            return await _dbSet
                .Include(c => c.Usuario)
                .Where(c => c.PaseadorId == paseadorId)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetChatByUsuarioAndPaseadorAsync(int usuarioId, int paseadorId)
        {
            return await _dbSet
                .Where(c => c.UsuarioId == usuarioId && c.PaseadorId == paseadorId)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();
        }
    }
} 