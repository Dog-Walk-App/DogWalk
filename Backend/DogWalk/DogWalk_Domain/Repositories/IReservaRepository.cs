using DogWalk_Domain.Entities;
using DogWalk_Domain.Enums;

namespace DogWalk_Domain.Repositories
{
    public interface IReservaRepository : IRepository<Reserva>
    {
        Task<IEnumerable<Reserva>> GetReservasByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<Reserva>> GetReservasByPaseadorIdAsync(int paseadorId);
        Task<IEnumerable<Reserva>> GetReservasByEstadoAsync(ReservaStatus estado);
        Task<IEnumerable<Reserva>> GetReservasByFechaAsync(DateTime fecha);
    }
} 