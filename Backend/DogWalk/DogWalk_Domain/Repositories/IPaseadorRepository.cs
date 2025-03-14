using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IPaseadorRepository : IRepository<Paseador>
    {
        Task<Paseador?> GetByEmailAsync(string email);
        Task<IEnumerable<Paseador>> GetPaseadoresByValoracionAsync(decimal valoracionMinima);
        Task<IEnumerable<Paseador>> GetPaseadoresCercaDeUbicacionAsync(double latitud, double longitud, double distanciaMaximaKm);
    }
} 