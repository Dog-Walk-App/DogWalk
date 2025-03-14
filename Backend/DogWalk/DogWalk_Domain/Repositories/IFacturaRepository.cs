using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Repositories
{
    public interface IFacturaRepository : IRepository<Factura>
    {
        Task<IEnumerable<Factura>> GetFacturasByUsuarioIdAsync(int usuarioId);
        Task<Factura?> GetFacturaWithDetallesAsync(int facturaId);
        Task<IEnumerable<Factura>> GetFacturasByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    }
} 