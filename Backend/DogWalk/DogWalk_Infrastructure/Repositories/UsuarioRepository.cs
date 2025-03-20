using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(DogWalkDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email.Value == email);
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosWithPerrosAsync()
        {
            return await _dbSet
                .Include(u => u.Perros)
                .ToListAsync();
        }
    }
} 