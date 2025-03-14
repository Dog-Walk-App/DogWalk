using DogWalk_Application.DTOs;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiciosController> _logger;

        public ServiciosController(IUnitOfWork unitOfWork, ILogger<ServiciosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Servicios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicioDto>>> GetAll()
        {
            try
            {
                var servicios = await _unitOfWork.Servicios.GetAllAsync();
                var serviciosDto = servicios.Select(s => new ServicioDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion
                });

                return Ok(serviciosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los servicios");
                return StatusCode(500, "Error interno del servidor al obtener los servicios");
            }
        }

        // GET: api/Servicios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServicioDto>> GetById(int id)
        {
            try
            {
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {id}");
                }

                var servicioDto = new ServicioDto
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion
                };

                return Ok(servicioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el servicio con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al obtener el servicio");
            }
        }

        // GET: api/Servicios/Paseador/5
        [HttpGet("Paseador/{paseadorId}")]
        public async Task<ActionResult<IEnumerable<ServicioWithPrecioDto>>> GetServiciosByPaseador(int paseadorId)
        {
            try
            {
                // Verificar si el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró el paseador con ID {paseadorId}");
                }

                // Obtener los servicios del paseador
                var servicios = await _unitOfWork.Servicios.GetServiciosByPaseadorIdAsync(paseadorId);
                
                // Obtener los precios para cada servicio del paseador
                var serviciosConPrecio = new List<ServicioWithPrecioDto>();
                
                foreach (var servicio in servicios)
                {
                    // Buscar el precio para este servicio y paseador
                    var precios = await _unitOfWork.Servicios.FindAsync(s => 
                        s.Id == servicio.Id && 
                        s.Precios.Any(p => p.PaseadorId == paseadorId));
                    
                    var precio = precios.FirstOrDefault()?.Precios
                        .FirstOrDefault(p => p.PaseadorId == paseadorId && p.ServicioId == servicio.Id);
                    
                    serviciosConPrecio.Add(new ServicioWithPrecioDto
                    {
                        Id = servicio.Id,
                        Nombre = servicio.Nombre,
                        Descripcion = servicio.Descripcion,
                        Precio = precio?.Valor ?? 0
                    });
                }

                return Ok(serviciosConPrecio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los servicios del paseador con ID {PaseadorId}", paseadorId);
                return StatusCode(500, "Error interno del servidor al obtener los servicios del paseador");
            }
        }

        // POST: api/Servicios
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServicioDto>> Create(ServicioCreateDto servicioDto)
        {
            try
            {
                // Verificar si ya existe un servicio con el mismo nombre
                var servicioExistente = await _unitOfWork.Servicios.FindAsync(s => s.Nombre == servicioDto.Nombre);
                if (servicioExistente.Any())
                {
                    return BadRequest($"Ya existe un servicio con el nombre '{servicioDto.Nombre}'");
                }

                var servicio = new Servicio
                {
                    Nombre = servicioDto.Nombre,
                    Descripcion = servicioDto.Descripcion
                };

                await _unitOfWork.Servicios.AddAsync(servicio);
                await _unitOfWork.SaveChangesAsync();

                var nuevoServicioDto = new ServicioDto
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion
                };

                return CreatedAtAction(nameof(GetById), new { id = servicio.Id }, nuevoServicioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un nuevo servicio");
                return StatusCode(500, "Error interno del servidor al crear el servicio");
            }
        }

        // PUT: api/Servicios/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ServicioUpdateDto servicioDto)
        {
            try
            {
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {id}");
                }

                // Verificar si ya existe otro servicio con el mismo nombre
                var servicioExistente = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Nombre == servicioDto.Nombre && s.Id != id);
                
                if (servicioExistente.Any())
                {
                    return BadRequest($"Ya existe otro servicio con el nombre '{servicioDto.Nombre}'");
                }

                servicio.Nombre = servicioDto.Nombre;
                servicio.Descripcion = servicioDto.Descripcion;

                _unitOfWork.Servicios.Update(servicio);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el servicio con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al actualizar el servicio");
            }
        }

        // DELETE: api/Servicios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {id}");
                }

                // Verificar si el servicio está siendo utilizado en reservas
                var reservasConServicio = await _unitOfWork.Reservas.FindAsync(r => r.ServicioId == id);
                if (reservasConServicio.Any())
                {
                    return BadRequest("No se puede eliminar el servicio porque está siendo utilizado en reservas");
                }

                _unitOfWork.Servicios.Remove(servicio);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el servicio con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al eliminar el servicio");
            }
        }

        // POST: api/Servicios/AsignarPrecio
        [HttpPost("AsignarPrecio")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> AsignarPrecio([FromBody] AsignarPrecioDto asignarPrecioDto)
        {
            try
            {
                // Obtener el ID del paseador autenticado
                var paseadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (paseadorIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                if (!int.TryParse(paseadorIdClaim.Value, out int paseadorId))
                {
                    return BadRequest("ID de paseador inválido");
                }

                // Verificar si el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró el paseador con ID {paseadorId}");
                }

                // Verificar si el servicio existe
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(asignarPrecioDto.ServicioId);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {asignarPrecioDto.ServicioId}");
                }

                // Buscar si ya existe un precio para este servicio y paseador
                var precios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Id == asignarPrecioDto.ServicioId && 
                    s.Precios.Any(p => p.PaseadorId == paseadorId));
                
                var precioExistente = precios.FirstOrDefault()?.Precios
                    .FirstOrDefault(p => p.PaseadorId == paseadorId && p.ServicioId == asignarPrecioDto.ServicioId);

                if (precioExistente != null)
                {
                    // Actualizar el precio existente
                    precioExistente.Valor = asignarPrecioDto.Precio;
                    _unitOfWork.Servicios.Update(servicio);
                }
                else
                {
                    // Crear un nuevo precio
                    var nuevoPrecio = new Precio
                    {
                        PaseadorId = paseadorId,
                        ServicioId = asignarPrecioDto.ServicioId,
                        Valor = asignarPrecioDto.Precio
                    };

                    servicio.Precios.Add(nuevoPrecio);
                    _unitOfWork.Servicios.Update(servicio);
                }

                await _unitOfWork.SaveChangesAsync();
                return Ok("Precio asignado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar precio al servicio");
                return StatusCode(500, "Error interno del servidor al asignar precio al servicio");
            }
        }
    }
} 