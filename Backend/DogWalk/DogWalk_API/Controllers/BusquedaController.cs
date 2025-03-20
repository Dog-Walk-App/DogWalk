using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DogWalk_Application.DTOs;
using DogWalk_Domain.Repositories;
using System.Linq;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusquedaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BusquedaController> _logger;

        public BusquedaController(IUnitOfWork unitOfWork, ILogger<BusquedaController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Busqueda/Paseadores
        [HttpGet("Paseadores")]
        public async Task<ActionResult<IEnumerable<PaseadorDto>>> BuscarPaseadores([FromQuery] BusquedaPaseadorDto busquedaDto)
        {
            try
            {
                var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
                var paseadoresDto = new List<PaseadorDto>();

                foreach (var paseador in paseadores)
                {
                    // Filtrar por valoración mínima si se especifica
                    if (busquedaDto.ValoracionMinima.HasValue && paseador.ValoracionMedia < busquedaDto.ValoracionMinima.Value)
                    {
                        continue;
                    }

                    // Filtrar por servicio si se especifica
                    if (busquedaDto.ServicioId.HasValue)
                    {
                        var serviciosPaseador = await _unitOfWork.Paseadores.GetServiciosByPaseadorIdAsync(paseador.Id);
                        if (!serviciosPaseador.Any(s => s.Id == busquedaDto.ServicioId.Value))
                        {
                            continue;
                        }
                    }

                    // Calcular distancia si se especifican coordenadas
                    double distancia = 0;
                    if (busquedaDto.Latitud.HasValue && busquedaDto.Longitud.HasValue)
                    {
                        distancia = CalcularDistancia(
                            busquedaDto.Latitud.Value, 
                            busquedaDto.Longitud.Value, 
                            paseador.Latitud, 
                            paseador.Longitud
                        );

                        // Filtrar por distancia máxima si se especifica
                        if (busquedaDto.DistanciaMaxima.HasValue && distancia > busquedaDto.DistanciaMaxima.Value)
                        {
                            continue;
                        }
                    }

                    // Obtener servicios con precios específicos para este paseador
                    var servicios = await _unitOfWork.Servicios.GetAllAsync();
                    var serviciosDto = new List<ServicioDto>();
                    
                    foreach (var s in servicios)
                    {
                        var precio = await _unitOfWork.Paseadores.GetPrecioServicioAsync(paseador.Id, s.Id);
                        serviciosDto.Add(new ServicioDto
                        {
                            Id = s.Id,
                            Nombre = s.Nombre,
                            Descripcion = s.Descripcion,
                            Precio = precio
                        });
                    }

                    var paseadorDto = new PaseadorDto
                    {
                        Id = paseador.Id,
                        Nombre = paseador.Nombre,
                        Apellido = paseador.Apellido,
                        Email = paseador.Email.ToString(),
                        Telefono = paseador.Telefono.ToString(),
                        Direccion = paseador.Direccion,
                        Latitud = paseador.Latitud,
                        Longitud = paseador.Longitud,
                        ValoracionMedia = paseador.ValoracionMedia,
                        Distancia = distancia,
                        Servicios = serviciosDto
                    };

                    paseadoresDto.Add(paseadorDto);
                }

                // Ordenar por distancia si se especificaron coordenadas
                if (busquedaDto.Latitud.HasValue && busquedaDto.Longitud.HasValue)
                {
                    paseadoresDto = paseadoresDto.OrderBy(p => p.Distancia).ToList();
                }
                // Si no, ordenar por valoración
                else
                {
                    paseadoresDto = paseadoresDto.OrderByDescending(p => p.ValoracionMedia).ToList();
                }

                return Ok(paseadoresDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar paseadores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Busqueda/PaseadoresCercanos
        [HttpGet("PaseadoresCercanos")]
        public async Task<ActionResult<IEnumerable<PaseadorDto>>> BuscarPaseadoresCercanos([FromQuery] BusquedaPaseadorCercanoDto busquedaDto)
        {
            try
            {
                if (!busquedaDto.Latitud.HasValue || !busquedaDto.Longitud.HasValue)
                {
                    return BadRequest("Se requieren las coordenadas (latitud y longitud) para buscar paseadores cercanos");
                }

                var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
                var paseadoresDto = new List<PaseadorDto>();

                // Establecer distancia máxima por defecto si no se especifica
                double distanciaMaxima = busquedaDto.DistanciaMaxima.HasValue ? busquedaDto.DistanciaMaxima.Value : 5; // 5 km por defecto

                foreach (var paseador in paseadores)
                {
                    // Calcular distancia
                    double distancia = CalcularDistancia(
                        busquedaDto.Latitud.Value, 
                        busquedaDto.Longitud.Value, 
                        paseador.Latitud, 
                        paseador.Longitud
                    );

                    // Filtrar por distancia máxima
                    if (distancia > distanciaMaxima)
                    {
                        continue;
                    }

                    // Obtener servicios con precios específicos para este paseador
                    var servicios = await _unitOfWork.Servicios.GetAllAsync();
                    var serviciosDto = new List<ServicioDto>();
                    
                    foreach (var s in servicios)
                    {
                        var precio = await _unitOfWork.Paseadores.GetPrecioServicioAsync(paseador.Id, s.Id);
                        serviciosDto.Add(new ServicioDto
                        {
                            Id = s.Id,
                            Nombre = s.Nombre,
                            Descripcion = s.Descripcion,
                            Precio = precio
                        });
                    }

                    var paseadorDto = new PaseadorDto
                    {
                        Id = paseador.Id,
                        Nombre = paseador.Nombre,
                        Apellido = paseador.Apellido,
                        Email = paseador.Email.ToString(),
                        Telefono = paseador.Telefono.ToString(),
                        Direccion = paseador.Direccion,
                        Latitud = paseador.Latitud,
                        Longitud = paseador.Longitud,
                        ValoracionMedia = paseador.ValoracionMedia,
                        Distancia = distancia,
                        Servicios = serviciosDto
                    };

                    paseadoresDto.Add(paseadorDto);
                }

                // Ordenar por distancia
                paseadoresDto = paseadoresDto.OrderBy(p => p.Distancia).ToList();

                return Ok(paseadoresDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar paseadores cercanos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Busqueda/Servicios
        [HttpGet("Servicios")]
        public async Task<ActionResult<IEnumerable<ServicioDto>>> BuscarServicios([FromQuery] BusquedaServicioDto busquedaDto)
        {
            try
            {
                var servicios = await _unitOfWork.Servicios.GetAllAsync();
                var serviciosDto = new List<ServicioDto>();

                foreach (var servicio in servicios)
                {
                    // Filtrar por nombre si se especifica
                    if (!string.IsNullOrEmpty(busquedaDto.Nombre) && 
                        !servicio.Nombre.Contains(busquedaDto.Nombre, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var servicioDto = new ServicioDto
                    {
                        Id = servicio.Id,
                        Nombre = servicio.Nombre,
                        Descripcion = servicio.Descripcion
                    };

                    serviciosDto.Add(servicioDto);
                }

                return Ok(serviciosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar servicios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Método para calcular la distancia entre dos puntos geográficos usando la fórmula de Haversine
        private double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
        {
            const double radioTierra = 6371; // Radio de la Tierra en kilómetros
            
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distancia = radioTierra * c;
            
            return distancia;
        }

        private double ToRadians(double grados)
        {
            return grados * Math.PI / 180;
        }
    }
} 