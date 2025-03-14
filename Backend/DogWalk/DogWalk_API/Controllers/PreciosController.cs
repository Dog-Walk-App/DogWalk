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
    public class PreciosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PreciosController> _logger;

        public PreciosController(IUnitOfWork unitOfWork, ILogger<PreciosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Precios/Paseador/5
        [HttpGet("Paseador/{paseadorId}")]
        public async Task<ActionResult<IEnumerable<PrecioDto>>> GetPreciosByPaseador(int paseadorId)
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
                
                var preciosDto = new List<PrecioDto>();
                
                foreach (var servicio in servicios)
                {
                    // Buscar el precio para este servicio y paseador
                    var precios = await _unitOfWork.Servicios.FindAsync(s => 
                        s.Id == servicio.Id && 
                        s.Precios.Any(p => p.PaseadorId == paseadorId));
                    
                    var precio = precios.FirstOrDefault()?.Precios
                        .FirstOrDefault(p => p.PaseadorId == paseadorId && p.ServicioId == servicio.Id);
                    
                    if (precio != null)
                    {
                        preciosDto.Add(new PrecioDto
                        {
                            // Usamos 0 como valor temporal para el ID ya que no podemos acceder a él directamente
                            Id = 0,
                            PaseadorId = precio.PaseadorId,
                            ServicioId = precio.ServicioId,
                            NombreServicio = servicio.Nombre,
                            Valor = precio.Valor
                        });
                    }
                }

                return Ok(preciosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los precios del paseador con ID {PaseadorId}", paseadorId);
                return StatusCode(500, "Error interno del servidor al obtener los precios del paseador");
            }
        }

        // GET: api/Precios/Servicio/5
        [HttpGet("Servicio/{servicioId}")]
        public async Task<ActionResult<IEnumerable<PrecioDto>>> GetPreciosByServicio(int servicioId)
        {
            try
            {
                // Verificar si el servicio existe
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(servicioId);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {servicioId}");
                }

                // Obtener los precios para este servicio
                var servicios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Id == servicioId && 
                    s.Precios.Any());
                
                var preciosDto = new List<PrecioDto>();
                
                foreach (var s in servicios)
                {
                    foreach (var precio in s.Precios.Where(p => p.ServicioId == servicioId))
                    {
                        // Obtener el paseador para mostrar su nombre
                        var paseador = await _unitOfWork.Paseadores.GetByIdAsync(precio.PaseadorId);
                        
                        preciosDto.Add(new PrecioDto
                        {
                            // Usamos 0 como valor temporal para el ID ya que no podemos acceder a él directamente
                            Id = 0,
                            PaseadorId = precio.PaseadorId,
                            NombrePaseador = paseador?.Nombre + " " + paseador?.Apellido,
                            ServicioId = precio.ServicioId,
                            NombreServicio = s.Nombre,
                            Valor = precio.Valor
                        });
                    }
                }

                return Ok(preciosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los precios del servicio con ID {ServicioId}", servicioId);
                return StatusCode(500, "Error interno del servidor al obtener los precios del servicio");
            }
        }

        // POST: api/Precios
        [HttpPost]
        [Authorize(Roles = "Paseador")]
        public async Task<ActionResult<PrecioDto>> Create(PrecioCreateDto precioDto)
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
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(precioDto.ServicioId);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {precioDto.ServicioId}");
                }

                // Verificar si ya existe un precio para este servicio y paseador
                var precios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Id == precioDto.ServicioId && 
                    s.Precios.Any(p => p.PaseadorId == paseadorId));
                
                var precioExistente = precios.FirstOrDefault()?.Precios
                    .FirstOrDefault(p => p.PaseadorId == paseadorId && p.ServicioId == precioDto.ServicioId);

                if (precioExistente != null)
                {
                    return BadRequest($"Ya existe un precio para el servicio con ID {precioDto.ServicioId}");
                }

                // Crear un nuevo precio
                var nuevoPrecio = new Precio
                {
                    PaseadorId = paseadorId,
                    ServicioId = precioDto.ServicioId,
                    Valor = precioDto.Valor
                };

                servicio.Precios.Add(nuevoPrecio);
                _unitOfWork.Servicios.Update(servicio);
                await _unitOfWork.SaveChangesAsync();

                var nuevoPrecioDto = new PrecioDto
                {
                    // Usamos 0 como valor temporal para el ID ya que no podemos acceder a él directamente
                    Id = 0,
                    PaseadorId = nuevoPrecio.PaseadorId,
                    ServicioId = nuevoPrecio.ServicioId,
                    NombreServicio = servicio.Nombre,
                    Valor = nuevoPrecio.Valor
                };

                return CreatedAtAction(nameof(GetPreciosByPaseador), new { paseadorId = paseadorId }, nuevoPrecioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un nuevo precio");
                return StatusCode(500, "Error interno del servidor al crear el precio");
            }
        }

        // PUT: api/Precios/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> Update(int id, PrecioUpdateDto precioDto)
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

                // Buscar el servicio que contiene el precio
                // Modificamos la consulta para no usar el ID del precio
                var servicios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Precios.Any(p => p.PaseadorId == paseadorId));
                
                // Buscamos el servicio que contiene un precio con el paseador especificado
                var servicio = servicios.FirstOrDefault();
                if (servicio == null)
                {
                    return NotFound($"No se encontraron precios para el paseador actual");
                }

                // Buscamos el precio por el ID del servicio y del paseador
                // Ya que no podemos acceder directamente al ID del precio
                var precio = servicio.Precios.FirstOrDefault(p => p.PaseadorId == paseadorId);
                if (precio == null)
                {
                    return NotFound($"No se encontró el precio para el paseador actual");
                }

                // Verificar que el precio pertenece al paseador autenticado
                if (precio.PaseadorId != paseadorId)
                {
                    return Forbid("No tiene permiso para actualizar este precio");
                }

                precio.Valor = precioDto.Valor;
                _unitOfWork.Servicios.Update(servicio);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el precio");
                return StatusCode(500, "Error interno del servidor al actualizar el precio");
            }
        }

        // DELETE: api/Precios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> Delete(int id)
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

                // Buscar el servicio que contiene el precio
                // Modificamos la consulta para no usar el ID del precio
                var servicios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Precios.Any(p => p.PaseadorId == paseadorId));
                
                // Buscamos el servicio que contiene un precio con el paseador especificado
                var servicio = servicios.FirstOrDefault();
                if (servicio == null)
                {
                    return NotFound($"No se encontraron precios para el paseador actual");
                }

                // Buscamos el precio por el ID del servicio y del paseador
                // Ya que no podemos acceder directamente al ID del precio
                var precio = servicio.Precios.FirstOrDefault(p => p.PaseadorId == paseadorId);
                if (precio == null)
                {
                    return NotFound($"No se encontró el precio para el paseador actual");
                }

                // Verificar que el precio pertenece al paseador autenticado
                if (precio.PaseadorId != paseadorId)
                {
                    return Forbid("No tiene permiso para eliminar este precio");
                }

                // Verificar si el precio está siendo utilizado en reservas
                var reservasConPrecio = await _unitOfWork.Reservas.FindAsync(r => 
                    r.PaseadorId == paseadorId && 
                    r.ServicioId == precio.ServicioId);
                
                if (reservasConPrecio.Any())
                {
                    return BadRequest("No se puede eliminar el precio porque está siendo utilizado en reservas");
                }

                servicio.Precios.Remove(precio);
                _unitOfWork.Servicios.Update(servicio);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el precio");
                return StatusCode(500, "Error interno del servidor al eliminar el precio");
            }
        }
    }
} 