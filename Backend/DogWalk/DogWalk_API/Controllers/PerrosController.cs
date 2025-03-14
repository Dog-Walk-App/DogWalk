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
    [Authorize]
    public class PerrosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PerrosController> _logger;

        public PerrosController(IUnitOfWork unitOfWork, ILogger<PerrosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PerroDto>>> GetAll()
        {
            try
            {
                var perros = await _unitOfWork.Perros.GetAllAsync();
                var perrosDto = perros.Select(p => new PerroDto
                {
                    Id = p.Id,
                    UsuarioId = p.UsuarioId,
                    Nombre = p.Nombre,
                    Raza = p.Raza,
                    Edad = p.Edad,
                    GpsUbicacion = p.GpsUbicacion,
                    NombreUsuario = p.Usuario.Nombre + " " + p.Usuario.Apellido
                }).ToList();

                return Ok(perrosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los perros: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PerroDto>>> GetByUsuarioId(int usuarioId)
        {
            try
            {
                // Verificar si el usuario actual tiene permiso para ver estos perros
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != usuarioId)
                {
                    return Forbid("No tienes permiso para ver los perros de este usuario");
                }

                var perros = await _unitOfWork.Perros.GetPerrosByUsuarioIdAsync(usuarioId);
                var perrosDto = perros.Select(p => new PerroDto
                {
                    Id = p.Id,
                    UsuarioId = p.UsuarioId,
                    Nombre = p.Nombre,
                    Raza = p.Raza,
                    Edad = p.Edad,
                    GpsUbicacion = p.GpsUbicacion,
                    NombreUsuario = p.Usuario.Nombre + " " + p.Usuario.Apellido
                }).ToList();

                return Ok(perrosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los perros del usuario {UsuarioId}: {Message}", usuarioId, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PerroWithFotosDto>> GetById(int id)
        {
            try
            {
                var perro = await _unitOfWork.Perros.GetPerroWithFotosAsync(id);
                if (perro == null)
                {
                    return NotFound($"No se encontró el perro con ID {id}");
                }

                // Verificar si el usuario actual tiene permiso para ver este perro
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != perro.UsuarioId)
                {
                    return Forbid("No tienes permiso para ver este perro");
                }

                var perroDto = new PerroWithFotosDto
                {
                    Id = perro.Id,
                    UsuarioId = perro.UsuarioId,
                    Nombre = perro.Nombre,
                    Raza = perro.Raza,
                    Edad = perro.Edad,
                    GpsUbicacion = perro.GpsUbicacion,
                    NombreUsuario = perro.Usuario.Nombre + " " + perro.Usuario.Apellido,
                    Fotos = perro.Fotos.Select(f => new FotoPerroDto
                    {
                        Id = f.Id,
                        PerroId = f.PerroId,
                        UrlFoto = f.UrlFoto,
                        Descripcion = f.Descripcion
                    }).ToList()
                };

                return Ok(perroDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perro con ID {PerroId}: {Message}", id, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PerroDto>> Create(PerroCreateDto perroDto)
        {
            try
            {
                // Verificar si el usuario actual tiene permiso para crear un perro para este usuario
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != perroDto.UsuarioId)
                {
                    return Forbid("No tienes permiso para crear un perro para este usuario");
                }

                // Verificar si el usuario existe
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(perroDto.UsuarioId);
                if (usuario == null)
                {
                    return BadRequest($"No existe un usuario con ID {perroDto.UsuarioId}");
                }

                var perro = new Perro
                {
                    UsuarioId = perroDto.UsuarioId,
                    Nombre = perroDto.Nombre,
                    Raza = perroDto.Raza,
                    Edad = perroDto.Edad,
                    GpsUbicacion = perroDto.GpsUbicacion
                };

                await _unitOfWork.Perros.AddAsync(perro);
                await _unitOfWork.SaveChangesAsync();

                var createdPerroDto = new PerroDto
                {
                    Id = perro.Id,
                    UsuarioId = perro.UsuarioId,
                    Nombre = perro.Nombre,
                    Raza = perro.Raza,
                    Edad = perro.Edad,
                    GpsUbicacion = perro.GpsUbicacion,
                    NombreUsuario = usuario.Nombre + " " + usuario.Apellido
                };

                return CreatedAtAction(nameof(GetById), new { id = perro.Id }, createdPerroDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un perro: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, PerroUpdateDto perroDto)
        {
            try
            {
                var perro = await _unitOfWork.Perros.GetByIdAsync(id);
                if (perro == null)
                {
                    return NotFound($"No se encontró el perro con ID {id}");
                }

                // Verificar si el usuario actual tiene permiso para actualizar este perro
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != perro.UsuarioId)
                {
                    return Forbid("No tienes permiso para actualizar este perro");
                }

                perro.Nombre = perroDto.Nombre;
                perro.Raza = perroDto.Raza;
                perro.Edad = perroDto.Edad;
                perro.GpsUbicacion = perroDto.GpsUbicacion;

                _unitOfWork.Perros.Update(perro);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perro con ID {PerroId}: {Message}", id, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var perro = await _unitOfWork.Perros.GetByIdAsync(id);
                if (perro == null)
                {
                    return NotFound($"No se encontró el perro con ID {id}");
                }

                // Verificar si el usuario actual tiene permiso para eliminar este perro
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != perro.UsuarioId)
                {
                    return Forbid("No tienes permiso para eliminar este perro");
                }

                _unitOfWork.Perros.Remove(perro);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el perro con ID {PerroId}: {Message}", id, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{perroId}/fotos")]
        [Authorize]
        public async Task<ActionResult<FotoPerroDto>> AddFoto(int perroId, [FromBody] FotoPerroCreateDto fotoDto)
        {
            try
            {
                var perro = await _unitOfWork.Perros.GetByIdAsync(perroId);
                if (perro == null)
                {
                    return NotFound($"No se encontró el perro con ID {perroId}");
                }

                // Verificar si el usuario actual tiene permiso para añadir fotos a este perro
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != perro.UsuarioId)
                {
                    return Forbid("No tienes permiso para añadir fotos a este perro");
                }

                var foto = new FotoPerro
                {
                    PerroId = perroId,
                    UrlFoto = fotoDto.UrlFoto,
                    Descripcion = fotoDto.Descripcion
                };

                // Añadir la foto directamente a la colección de fotos del perro
                perro.Fotos.Add(foto);
                await _unitOfWork.SaveChangesAsync();

                var createdFotoDto = new FotoPerroDto
                {
                    Id = foto.Id,
                    PerroId = foto.PerroId,
                    UrlFoto = foto.UrlFoto,
                    Descripcion = foto.Descripcion
                };

                return CreatedAtAction(nameof(GetById), new { id = perroId }, createdFotoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al añadir una foto al perro con ID {PerroId}: {Message}", perroId, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{perroId}/fotos/{fotoId}")]
        [Authorize]
        public async Task<IActionResult> DeleteFoto(int perroId, int fotoId)
        {
            try
            {
                var perro = await _unitOfWork.Perros.GetPerroWithFotosAsync(perroId);
                if (perro == null)
                {
                    return NotFound($"No se encontró el perro con ID {perroId}");
                }

                // Verificar si el usuario actual tiene permiso para eliminar fotos de este perro
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado");
                }

                if (userRole != "Admin" && int.Parse(userIdClaim) != perro.UsuarioId)
                {
                    return Forbid("No tienes permiso para eliminar fotos de este perro");
                }

                var foto = perro.Fotos.FirstOrDefault(f => f.Id == fotoId);
                if (foto == null)
                {
                    return NotFound($"No se encontró la foto con ID {fotoId} para el perro con ID {perroId}");
                }

                perro.Fotos.Remove(foto);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la foto con ID {FotoId} del perro con ID {PerroId}: {Message}", fotoId, perroId, ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 