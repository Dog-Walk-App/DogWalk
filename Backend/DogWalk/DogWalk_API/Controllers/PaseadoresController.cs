using DogWalk_Application.DTOs;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaseadoresController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaseadoresController> _logger;

        public PaseadoresController(IUnitOfWork unitOfWork, ILogger<PaseadoresController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PaseadorDto>>> GetAll()
        {
            try
            {
                var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
                var paseadoresDto = paseadores.Select(p => new PaseadorDto
                {
                    Id = p.Id,
                    Dni = p.Dni.Value,
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Direccion = p.Direccion,
                    Email = p.Email.Value,
                    Telefono = p.Telefono.Value,
                    ValoracionGeneral = p.ValoracionGeneral,
                    Latitud = p.Ubicacion.Latitud,
                    Longitud = p.Ubicacion.Longitud
                });

                return Ok(paseadoresDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los paseadores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaseadorDto>> GetById(int id)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(id);
                if (paseador == null)
                {
                    return NotFound($"Paseador con ID {id} no encontrado");
                }

                var paseadorDto = new PaseadorDto
                {
                    Id = paseador.Id,
                    Dni = paseador.Dni.Value,
                    Nombre = paseador.Nombre,
                    Apellido = paseador.Apellido,
                    Direccion = paseador.Direccion,
                    Email = paseador.Email.Value,
                    Telefono = paseador.Telefono.Value,
                    ValoracionGeneral = paseador.ValoracionGeneral,
                    Latitud = paseador.Ubicacion.Latitud,
                    Longitud = paseador.Ubicacion.Longitud
                };

                return Ok(paseadorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el paseador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("cercanos")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PaseadorDto>>> GetPaseadoresCercanos(double latitud, double longitud, double distanciaMaximaKm = 5)
        {
            try
            {
                var paseadores = await _unitOfWork.Paseadores.GetPaseadoresCercaDeUbicacionAsync(latitud, longitud, distanciaMaximaKm);
                var paseadoresDto = paseadores.Select(p => new PaseadorDto
                {
                    Id = p.Id,
                    Dni = p.Dni.Value,
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Direccion = p.Direccion,
                    Email = p.Email.Value,
                    Telefono = p.Telefono.Value,
                    ValoracionGeneral = p.ValoracionGeneral,
                    Latitud = p.Ubicacion.Latitud,
                    Longitud = p.Ubicacion.Longitud
                });

                return Ok(paseadoresDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paseadores cercanos a la ubicación ({Latitud}, {Longitud})", latitud, longitud);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("valoracion")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PaseadorDto>>> GetPaseadoresPorValoracion(decimal valoracionMinima = 4.0m)
        {
            try
            {
                var paseadores = await _unitOfWork.Paseadores.GetPaseadoresByValoracionAsync(valoracionMinima);
                var paseadoresDto = paseadores.Select(p => new PaseadorDto
                {
                    Id = p.Id,
                    Dni = p.Dni.Value,
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Direccion = p.Direccion,
                    Email = p.Email.Value,
                    Telefono = p.Telefono.Value,
                    ValoracionGeneral = p.ValoracionGeneral,
                    Latitud = p.Ubicacion.Latitud,
                    Longitud = p.Ubicacion.Longitud
                });

                return Ok(paseadoresDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paseadores con valoración mínima de {ValoracionMinima}", valoracionMinima);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<PaseadorDto>> Create(PaseadorCreateDto paseadorDto)
        {
            try
            {
                // Verificar si ya existe un paseador con el mismo email
                var existingPaseador = await _unitOfWork.Paseadores.GetByEmailAsync(paseadorDto.Email);
                if (existingPaseador != null)
                {
                    return BadRequest("Ya existe un paseador con este email");
                }

                // Verificar si ya existe un paseador con el mismo DNI
                var existingPaseadores = await _unitOfWork.Paseadores.FindAsync(p => p.Dni.Value == paseadorDto.Dni);
                if (existingPaseadores.Any())
                {
                    return BadRequest("Ya existe un paseador con este DNI");
                }

                // Crear el objeto de dominio
                var email = Email.Create(paseadorDto.Email);
                var dni = Dni.Create(paseadorDto.Dni);
                var telefono = Telefono.Create(paseadorDto.Telefono);
                var ubicacion = Coordenadas.Create(paseadorDto.Latitud, paseadorDto.Longitud);
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(paseadorDto.Password);

                var paseador = new Paseador
                {
                    Dni = dni,
                    Nombre = paseadorDto.Nombre,
                    Apellido = paseadorDto.Apellido,
                    Direccion = paseadorDto.Direccion,
                    Email = email,
                    Password = hashedPassword,
                    Telefono = telefono,
                    ValoracionGeneral = 0, // Valoración inicial
                    Ubicacion = ubicacion
                };

                await _unitOfWork.Paseadores.AddAsync(paseador);
                await _unitOfWork.SaveChangesAsync();

                var createdPaseadorDto = new PaseadorDto
                {
                    Id = paseador.Id,
                    Dni = paseador.Dni.Value,
                    Nombre = paseador.Nombre,
                    Apellido = paseador.Apellido,
                    Direccion = paseador.Direccion,
                    Email = paseador.Email.Value,
                    Telefono = paseador.Telefono.Value,
                    ValoracionGeneral = paseador.ValoracionGeneral,
                    Latitud = paseador.Ubicacion.Latitud,
                    Longitud = paseador.Ubicacion.Longitud
                };

                return CreatedAtAction(nameof(GetById), new { id = paseador.Id }, createdPaseadorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el paseador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, PaseadorUpdateDto paseadorDto)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(id);
                if (paseador == null)
                {
                    return NotFound($"Paseador con ID {id} no encontrado");
                }

                // Verificar si el usuario actual tiene permiso para actualizar este paseador
                if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != id.ToString() && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Actualizar propiedades
                paseador.Nombre = paseadorDto.Nombre;
                paseador.Apellido = paseadorDto.Apellido;
                paseador.Direccion = paseadorDto.Direccion;
                paseador.Telefono = Telefono.Create(paseadorDto.Telefono);
                paseador.Ubicacion = Coordenadas.Create(paseadorDto.Latitud, paseadorDto.Longitud);

                _unitOfWork.Paseadores.Update(paseador);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el paseador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(id);
                if (paseador == null)
                {
                    return NotFound($"Paseador con ID {id} no encontrado");
                }

                _unitOfWork.Paseadores.Remove(paseador);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el paseador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, UsuarioChangePasswordDto passwordDto)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(id);
                if (paseador == null)
                {
                    return NotFound($"Paseador con ID {id} no encontrado");
                }

                // Verificar si el usuario actual tiene permiso para cambiar la contraseña
                if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != id.ToString() && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Verificar contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(passwordDto.OldPassword, paseador.Password))
                {
                    return BadRequest("La contraseña actual es incorrecta");
                }

                // Verificar que la nueva contraseña y la confirmación coincidan
                if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
                {
                    return BadRequest("La nueva contraseña y la confirmación no coinciden");
                }

                // Actualizar contraseña
                paseador.Password = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);

                _unitOfWork.Paseadores.Update(paseador);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña del paseador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 