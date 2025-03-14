using DogWalk_Domain.Common;
using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DogWalk_Infrastructure.Persistence
{
    public class DogWalkDbContext : DbContext
    {
        public DogWalkDbContext(DbContextOptions<DogWalkDbContext> options) : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Paseador> Paseadores { get; set; }
        public DbSet<Perro> Perros { get; set; }
        public DbSet<FotoPerro> FotosPerros { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Precio> Precios { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<ImagenArticulo> ImagenesArticulos { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Ranking> Rankings { get; set; }
        public DbSet<Opinion> Opiniones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configuración de clave primaria compuesta para Precio
            modelBuilder.Entity<Precio>()
                .HasKey(p => new { p.PaseadorId, p.ServicioId });

            // Configuración de clave primaria compuesta para Ranking
            modelBuilder.Entity<Ranking>()
                .HasKey(r => new { r.UsuarioId, r.PaseadorId });

            // Configuración de relaciones
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId);

            modelBuilder.Entity<Perro>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Perros)
                .HasForeignKey(p => p.UsuarioId);

            modelBuilder.Entity<FotoPerro>()
                .HasOne(f => f.Perro)
                .WithMany(p => p.Fotos)
                .HasForeignKey(f => f.PerroId);

            modelBuilder.Entity<Precio>()
                .HasOne(p => p.Paseador)
                .WithMany(p => p.Precios)
                .HasForeignKey(p => p.PaseadorId);

            modelBuilder.Entity<Precio>()
                .HasOne(p => p.Servicio)
                .WithMany(s => s.Precios)
                .HasForeignKey(p => p.ServicioId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Paseador)
                .WithMany(p => p.Reservas)
                .HasForeignKey(r => r.PaseadorId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Servicio)
                .WithMany(s => s.Reservas)
                .HasForeignKey(r => r.ServicioId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Perro)
                .WithMany(p => p.Reservas)
                .HasForeignKey(r => r.PerroId);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Horario)
                .WithMany(h => h.Reservas)
                .HasForeignKey(r => r.HorarioId);

            modelBuilder.Entity<ImagenArticulo>()
                .HasOne(i => i.Articulo)
                .WithMany(a => a.Imagenes)
                .HasForeignKey(i => i.ArticuloId);

            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.CarritoItems)
                .HasForeignKey(c => c.UsuarioId);

            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Articulo)
                .WithMany(a => a.CarritoItems)
                .HasForeignKey(c => c.ArticuloId);

            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Usuario)
                .WithMany(u => u.Facturas)
                .HasForeignKey(f => f.UsuarioId);

            modelBuilder.Entity<DetalleFactura>()
                .HasOne(d => d.Factura)
                .WithMany(f => f.Detalles)
                .HasForeignKey(d => d.FacturaId);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Chats)
                .HasForeignKey(c => c.UsuarioId);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Paseador)
                .WithMany(p => p.Chats)
                .HasForeignKey(c => c.PaseadorId);

            modelBuilder.Entity<Ranking>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Rankings)
                .HasForeignKey(r => r.UsuarioId);

            modelBuilder.Entity<Ranking>()
                .HasOne(r => r.Paseador)
                .WithMany(p => p.Rankings)
                .HasForeignKey(r => r.PaseadorId);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.Perro)
                .WithMany(p => p.Opiniones)
                .HasForeignKey(o => o.PerroId);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.Paseador)
                .WithMany(p => p.Opiniones)
                .HasForeignKey(o => o.PaseadorId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Interceptar eventos de dominio antes de guardar cambios
            var entities = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity);

            var domainEvents = entities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            entities.ToList().ForEach(e => e.ClearDomainEvents());

            // Aquí se podrían publicar los eventos de dominio usando MediatR

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
} 