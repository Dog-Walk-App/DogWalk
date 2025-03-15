using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IHorarioRepository : IRepository<Horario>
    {
        Task<IEnumerable<Horario>> GetHorariosByPaseadorIdAsync(int paseadorId);
        Task<IEnumerable<Horario>> GetHorariosDisponiblesByPaseadorIdAsync(int paseadorId);
    }
} 