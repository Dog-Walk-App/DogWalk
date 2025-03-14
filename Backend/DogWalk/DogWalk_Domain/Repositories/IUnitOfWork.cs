namespace DogWalk_Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUsuarioRepository Usuarios { get; }
        IPaseadorRepository Paseadores { get; }
        IPerroRepository Perros { get; }
        IServicioRepository Servicios { get; }
        IReservaRepository Reservas { get; }
        IArticuloRepository Articulos { get; }
        ICarritoRepository Carritos { get; }
        IFacturaRepository Facturas { get; }
        IChatRepository Chats { get; }
        IRankingRepository Rankings { get; }
        IOpinionRepository Opiniones { get; }

        Task<int> SaveChangesAsync();
    }
} 