using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Persistence;

namespace DogWalk_Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DogWalkDbContext _context;
        private IUsuarioRepository? _usuarios;
        private IPaseadorRepository? _paseadores;
        private IPerroRepository? _perros;
        private IServicioRepository? _servicios;
        private IReservaRepository? _reservas;
        private IArticuloRepository? _articulos;
        private ICarritoRepository? _carritos;
        private IItemCarritoRepository? _itemsCarrito;
        private IFacturaRepository? _facturas;
        private IChatRepository? _chats;
        private IRankingRepository? _rankings;
        private IOpinionRepository? _opiniones;
        private IHorarioRepository? _horarios;

        public UnitOfWork(DogWalkDbContext context)
        {
            _context = context;
        }

        public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_context);
        public IPaseadorRepository Paseadores => _paseadores ??= new PaseadorRepository(_context);
        public IPerroRepository Perros => _perros ??= new PerroRepository(_context);
        public IServicioRepository Servicios => _servicios ??= new ServicioRepository(_context);
        public IReservaRepository Reservas => _reservas ??= new ReservaRepository(_context);
        public IArticuloRepository Articulos => _articulos ??= new ArticuloRepository(_context);
        public ICarritoRepository Carritos => _carritos ??= new CarritoRepository(_context);
        public IItemCarritoRepository ItemsCarrito => _itemsCarrito ??= new ItemCarritoRepository(_context);
        public IFacturaRepository Facturas => _facturas ??= new FacturaRepository(_context);
        public IChatRepository Chats => _chats ??= new ChatRepository(_context);
        public IRankingRepository Rankings => _rankings ??= new RankingRepository(_context);
        public IOpinionRepository Opiniones => _opiniones ??= new OpinionRepository(_context);
        public IHorarioRepository Horarios => _horarios ??= new HorarioRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 