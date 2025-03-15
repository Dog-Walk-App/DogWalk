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

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorariosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HorariosController> _logger;

        public HorariosController(IUnitOfWork unitOfWork, ILogger<HorariosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Horarios/Paseador/{paseadorId}
        [HttpGet("Paseador/{paseadorId}")]
        public async Task<ActionResult<IEnumerable<HorarioDto>>> GetHorariosByPaseadorId(int paseadorId)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró un paseador con el ID {paseadorId}");
                }

                var horarios = await _unitOfWork.Horarios.GetHorariosByPaseadorIdAsync(paseadorId);
                var horariosDto = new List<HorarioDto>();

                foreach (var horario in horarios)
                {
                    horariosDto.Add(new HorarioDto
                    {
                        Id = horario.Id,
                        PaseadorId = paseadorId,
                        FechaHora = horario.FechaHora,
                        Disponible = horario.Disponibilidad == DogWalk_Domain.Enums.DisponibilidadStatus.Disponible
                    });
                }

                return Ok(horariosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los horarios del paseador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Horarios/Disponibles/{paseadorId}
        [HttpGet("Disponibles/{paseadorId}")]
        public async Task<ActionResult<IEnumerable<HorarioDto>>> GetHorariosDisponiblesByPaseadorId(int paseadorId)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró un paseador con el ID {paseadorId}");
                }

                var horarios = await _unitOfWork.Horarios.GetHorariosDisponiblesByPaseadorIdAsync(paseadorId);
                var horariosDto = new List<HorarioDto>();

                foreach (var horario in horarios)
                {
                    horariosDto.Add(new HorarioDto
                    {
                        Id = horario.Id,
                        PaseadorId = paseadorId,
                        FechaHora = horario.FechaHora,
                        Disponible = true
                    });
                }

                return Ok(horariosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los horarios disponibles del paseador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Horarios
        [HttpPost]
        [Authorize(Roles = "Paseador")]
        public async Task<ActionResult<HorarioDto>> CreateHorario(HorarioCreateDto horarioDto)
        {
            try
            {
                var paseadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (paseadorId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró un paseador con el ID {paseadorId}");
                }

                var horario = new Horario
                {
                    PaseadorId = paseadorId,
                    FechaHora = horarioDto.FechaHora,
                    Disponibilidad = DogWalk_Domain.Enums.DisponibilidadStatus.Disponible
                };

                await _unitOfWork.Horarios.AddAsync(horario);
                await _unitOfWork.SaveChangesAsync();

                var createdHorarioDto = new HorarioDto
                {
                    Id = horario.Id,
                    PaseadorId = paseadorId,
                    FechaHora = horario.FechaHora,
                    Disponible = true
                };

                return CreatedAtAction(nameof(GetHorariosByPaseadorId), new { paseadorId = paseadorId }, createdHorarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un horario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Horarios/Rango
        [HttpPost("Rango")]
        [Authorize(Roles = "Paseador")]
        public async Task<ActionResult<IEnumerable<HorarioDto>>> CreateHorariosEnRango(HorarioRangoCreateDto rangoDto)
        {
            try
            {
                var paseadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (paseadorId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró un paseador con el ID {paseadorId}");
                }

                if (rangoDto.FechaInicio > rangoDto.FechaFin)
                {
                    return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");
                }

                var horariosCreados = new List<HorarioDto>();
                var fechaActual = rangoDto.FechaInicio;

                while (fechaActual <= rangoDto.FechaFin)
                {
                    // Solo crear horarios para los días seleccionados
                    if (rangoDto.DiasSeleccionados.Contains((int)fechaActual.DayOfWeek))
                    {
                        var horaInicio = rangoDto.HoraInicio;
                        var horaFin = rangoDto.HoraFin;

                        // Crear horarios para cada intervalo dentro del día
                        while (horaInicio < horaFin)
                        {
                            var fechaHora = fechaActual.Date.Add(horaInicio);
                            
                            var horario = new Horario
                            {
                                PaseadorId = paseadorId,
                                FechaHora = fechaHora,
                                Disponibilidad = DogWalk_Domain.Enums.DisponibilidadStatus.Disponible
                            };

                            await _unitOfWork.Horarios.AddAsync(horario);
                            
                            horariosCreados.Add(new HorarioDto
                            {
                                Id = horario.Id,
                                PaseadorId = paseadorId,
                                FechaHora = fechaHora,
                                Disponible = true
                            });

                            horaInicio = horaInicio.Add(TimeSpan.FromMinutes(rangoDto.IntervaloMinutos));
                        }
                    }

                    fechaActual = fechaActual.AddDays(1);
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(horariosCreados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear horarios en rango");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Horarios/{id}/Disponibilidad
        [HttpPut("{id}/Disponibilidad")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> UpdateDisponibilidad(int id, HorarioDisponibilidadDto disponibilidadDto)
        {
            try
            {
                var paseadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (paseadorId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var horario = await _unitOfWork.Horarios.GetByIdAsync(id);
                if (horario == null)
                {
                    return NotFound($"No se encontró un horario con el ID {id}");
                }

                if (horario.PaseadorId != paseadorId)
                {
                    return Forbid("No tienes permiso para modificar este horario");
                }

                horario.Disponibilidad = disponibilidadDto.Disponible 
                    ? DogWalk_Domain.Enums.DisponibilidadStatus.Disponible 
                    : DogWalk_Domain.Enums.DisponibilidadStatus.NoDisponible;

                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la disponibilidad del horario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Horarios/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> DeleteHorario(int id)
        {
            try
            {
                var paseadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (paseadorId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var horario = await _unitOfWork.Horarios.GetByIdAsync(id);
                if (horario == null)
                {
                    return NotFound($"No se encontró un horario con el ID {id}");
                }

                if (horario.PaseadorId != paseadorId)
                {
                    return Forbid("No tienes permiso para eliminar este horario");
                }

                // Verificar si el horario tiene reservas asociadas
                if (horario.Reservas.Count > 0)
                {
                    return BadRequest("No se puede eliminar un horario que tiene reservas asociadas");
                }

                _unitOfWork.Horarios.Remove(horario);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el horario");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 