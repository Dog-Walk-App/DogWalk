using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using DogWalk_Application.DTOs;
using DogWalk_Domain.Repositories;
using DogWalk_Domain.Entities;
using System.Security.Claims;
using System.Linq;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpinionesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OpinionesController> _logger;

        public OpinionesController(IUnitOfWork unitOfWork, ILogger<OpinionesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Opiniones/Paseador/{paseadorId}
        [HttpGet("Paseador/{paseadorId}")]
        public async Task<ActionResult<IEnumerable<OpinionDto>>> GetOpinionesByPaseadorId(int paseadorId)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró un paseador con el ID {paseadorId}");
                }

                var opiniones = await _unitOfWork.Opiniones.GetOpinionesByPaseadorIdAsync(paseadorId);
                var opinionesDto = new List<OpinionDto>();

                foreach (var opinion in opiniones)
                {
                    var usuario = await _unitOfWork.Usuarios.GetByIdAsync(opinion.UsuarioId);
                    
                    opinionesDto.Add(new OpinionDto
                    {
                        Id = opinion.Id,
                        PaseadorId = opinion.PaseadorId,
                        UsuarioId = opinion.UsuarioId,
                        Comentario = opinion.Comentario ?? string.Empty,
                        Valoracion = opinion.Valoracion,
                        FechaCreacion = opinion.FechaCreacion,
                        NombreUsuario = usuario?.Nombre ?? "Usuario desconocido",
                        NombrePaseador = paseador.Nombre
                    });
                }

                return Ok(opinionesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las opiniones del paseador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Opiniones/Usuario
        [HttpGet("Usuario")]
        [Authorize(Roles = "Usuario")]
        public async Task<ActionResult<IEnumerable<OpinionDto>>> GetOpinionesByUsuarioId()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return NotFound($"No se encontró un usuario con el ID {usuarioId}");
                }

                var opiniones = await _unitOfWork.Opiniones.GetOpinionesByUsuarioIdAsync(usuarioId);
                var opinionesDto = new List<OpinionDto>();

                foreach (var opinion in opiniones)
                {
                    var paseador = await _unitOfWork.Paseadores.GetByIdAsync(opinion.PaseadorId);
                    
                    opinionesDto.Add(new OpinionDto
                    {
                        Id = opinion.Id,
                        PaseadorId = opinion.PaseadorId,
                        UsuarioId = opinion.UsuarioId,
                        Comentario = opinion.Comentario ?? string.Empty,
                        Valoracion = opinion.Valoracion,
                        FechaCreacion = opinion.FechaCreacion,
                        NombreUsuario = usuario.Nombre,
                        NombrePaseador = paseador?.Nombre ?? "Paseador desconocido"
                    });
                }

                return Ok(opinionesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las opiniones del usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Opiniones
        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<ActionResult<OpinionDto>> CreateOpinion(OpinionCreateDto opinionDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return NotFound($"No se encontró un usuario con el ID {usuarioId}");
                }

                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(opinionDto.PaseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró un paseador con el ID {opinionDto.PaseadorId}");
                }

                // Verificar si el usuario ha completado al menos una reserva con este paseador
                var reservasCompletadas = await _unitOfWork.Reservas.GetReservasCompletadasByUsuarioAndPaseadorAsync(usuarioId, opinionDto.PaseadorId);
                if (reservasCompletadas.Count() == 0)
                {
                    return BadRequest("No puedes opinar sobre un paseador con el que no has completado ninguna reserva");
                }

                // Verificar si el usuario ya ha opinado sobre este paseador
                var opinionExistente = await _unitOfWork.Opiniones.GetOpinionByUsuarioAndPaseadorAsync(usuarioId, opinionDto.PaseadorId);
                if (opinionExistente != null)
                {
                    return BadRequest("Ya has opinado sobre este paseador. Si deseas modificar tu opinión, utiliza el método PUT");
                }

                if (opinionDto.Valoracion < 1 || opinionDto.Valoracion > 5)
                {
                    return BadRequest("La valoración debe estar entre 1 y 5");
                }

                var opinion = new Opinion
                {
                    PaseadorId = opinionDto.PaseadorId,
                    UsuarioId = usuarioId,
                    Comentario = opinionDto.Comentario,
                    Valoracion = opinionDto.Valoracion,
                    FechaCreacion = DateTime.Now
                };

                await _unitOfWork.Opiniones.AddAsync(opinion);
                await _unitOfWork.SaveChangesAsync();

                // Actualizar la valoración media del paseador
                await ActualizarValoracionMediaPaseador(opinionDto.PaseadorId);

                var createdOpinionDto = new OpinionDto
                {
                    Id = opinion.Id,
                    PaseadorId = opinion.PaseadorId,
                    UsuarioId = opinion.UsuarioId,
                    Comentario = opinion.Comentario ?? string.Empty,
                    Valoracion = opinion.Valoracion,
                    FechaCreacion = opinion.FechaCreacion,
                    NombreUsuario = usuario.Nombre,
                    NombrePaseador = paseador.Nombre
                };

                return CreatedAtAction(nameof(GetOpinionesByPaseadorId), new { paseadorId = opinionDto.PaseadorId }, createdOpinionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear una opinión");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Opiniones/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> UpdateOpinion(int id, OpinionUpdateDto opinionDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var opinion = await _unitOfWork.Opiniones.GetByIdAsync(id);
                if (opinion == null)
                {
                    return NotFound($"No se encontró una opinión con el ID {id}");
                }

                if (opinion.UsuarioId != usuarioId)
                {
                    return Forbid("No tienes permiso para modificar esta opinión");
                }

                if (opinionDto.Valoracion < 1 || opinionDto.Valoracion > 5)
                {
                    return BadRequest("La valoración debe estar entre 1 y 5");
                }

                opinion.Comentario = opinionDto.Comentario;
                opinion.Valoracion = opinionDto.Valoracion;

                await _unitOfWork.SaveChangesAsync();

                // Actualizar la valoración media del paseador
                await ActualizarValoracionMediaPaseador(opinion.PaseadorId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la opinión");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Opiniones/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Usuario,Admin")]
        public async Task<IActionResult> DeleteOpinion(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var esAdmin = User.IsInRole("Admin");
                
                if (usuarioId == 0 && !esAdmin)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var opinion = await _unitOfWork.Opiniones.GetByIdAsync(id);
                if (opinion == null)
                {
                    return NotFound($"No se encontró una opinión con el ID {id}");
                }

                if (opinion.UsuarioId != usuarioId && !esAdmin)
                {
                    return Forbid("No tienes permiso para eliminar esta opinión");
                }

                var paseadorId = opinion.PaseadorId;

                await _unitOfWork.Opiniones.DeleteAsync(opinion);
                await _unitOfWork.SaveChangesAsync();

                // Actualizar la valoración media del paseador
                await ActualizarValoracionMediaPaseador(paseadorId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la opinión");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Método auxiliar para actualizar la valoración media del paseador
        private async Task ActualizarValoracionMediaPaseador(int paseadorId)
        {
            var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
            if (paseador != null)
            {
                var opiniones = await _unitOfWork.Opiniones.GetOpinionesByPaseadorIdAsync(paseadorId);
                if (opiniones.Any())
                {
                    paseador.ValoracionMedia = opiniones.Average(o => o.Valoracion);
                }
                else
                {
                    paseador.ValoracionMedia = 0;
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
} 