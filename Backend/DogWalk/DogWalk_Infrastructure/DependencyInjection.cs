using DogWalk_Domain.Repositories;
using DogWalk_Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DogWalk_Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IPaseadorRepository, PaseadorRepository>();
            services.AddScoped<IServicioRepository, ServicioRepository>();
            // Los demás repositorios se registrarán a medida que se implementen

            return services;
        }
    }
} 