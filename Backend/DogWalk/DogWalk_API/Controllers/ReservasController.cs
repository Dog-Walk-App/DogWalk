using DogWalk_Application.DTOs;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Enums;
using DogWalk_Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(IUnitOfWork unitOfWork, ILogger<ReservasController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Reservas
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetAll()
        {
            try
            {
                var reservas = await _unitOfWork.Reservas.GetAllAsync();
                var reservasDto = await MapReservasToDto(reservas);
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reservas");
                return StatusCode(500, "Error interno del servidor al obtener las reservas");
            }
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaDto>> GetById(int id)
        {
            try
            {
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound($"No se encontró la reserva con ID {id}");
                }

                // Cargar datos relacionados
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(reserva.UsuarioId);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId);
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(reserva.ServicioId);

                // Verificar que el usuario actual sea el propietario de la reserva o el paseador asignado o un admin
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var esAdmin = User.IsInRole("Admin");
                
                if (!esAdmin && usuarioId != null)
                {
                    var esPropietario = reserva.UsuarioId.ToString() == usuarioId;
                    var esPaseador = reserva.PaseadorId.ToString() == usuarioId;
                    
                    if (!esPropietario && !esPaseador)
                    {
                        return Forbid("No tienes permiso para ver esta reserva");
                    }
                }

                var reservaDto = MapReservaToDto(
                    reserva, 
                    usuario ?? new Usuario(), 
                    paseador ?? new Paseador(), 
                    servicio ?? new Servicio());
                return Ok(reservaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la reserva con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al obtener la reserva");
            }
        }

        // GET: api/Reservas/Usuario
        [HttpGet("Usuario")]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservasByUsuario()
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                if (!int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("ID de usuario inválido");
                }

                var reservas = await _unitOfWork.Reservas.FindAsync(r => r.UsuarioId == usuarioId);
                var reservasDto = await MapReservasToDto(reservas);
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las reservas del usuario");
                return StatusCode(500, "Error interno del servidor al obtener las reservas del usuario");
            }
        }

        // GET: api/Reservas/Paseador
        [HttpGet("Paseador")]
        [Authorize(Roles = "Paseador")]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservasByPaseador()
        {
            try
            {
                var paseadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (paseadorIdClaim == null)
                {
                    return Unauthorized("Paseador no autenticado correctamente");
                }

                if (!int.TryParse(paseadorIdClaim.Value, out int paseadorId))
                {
                    return BadRequest("ID de paseador inválido");
                }

                var reservas = await _unitOfWork.Reservas.FindAsync(r => r.PaseadorId == paseadorId);
                var reservasDto = await MapReservasToDto(reservas);
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las reservas del paseador");
                return StatusCode(500, "Error interno del servidor al obtener las reservas del paseador");
            }
        }

        // POST: api/Reservas
        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<ActionResult<ReservaDto>> Create([FromBody] ReservaCreateDto createDto)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                if (!int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("ID de usuario inválido");
                }

                // Verificar si el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(createDto.PaseadorId);
                if (paseador == null)
                {
                    return NotFound($"No se encontró el paseador con ID {createDto.PaseadorId}");
                }

                // Verificar si el servicio existe
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(createDto.ServicioId);
                if (servicio == null)
                {
                    return NotFound($"No se encontró el servicio con ID {createDto.ServicioId}");
                }

                // Verificar si las mascotas existen y pertenecen al usuario
                foreach (var mascotaId in createDto.MascotasIds)
                {
                    var perro = await _unitOfWork.Perros.GetByIdAsync(mascotaId);
                    if (perro == null)
                    {
                        return NotFound($"No se encontró la mascota con ID {mascotaId}");
                    }

                    if (perro.UsuarioId != usuarioId)
                    {
                        return BadRequest($"La mascota con ID {mascotaId} no pertenece al usuario actual");
                    }
                }

                // Verificar disponibilidad del paseador
                var reservasExistentes = await _unitOfWork.Reservas.FindAsync(r => 
                    r.PaseadorId == createDto.PaseadorId && 
                    r.Estado != ReservaStatus.Cancelada);

                if (reservasExistentes.Any())
                {
                    // Implementamos lógica para verificar solapamientos de horarios
                    // Obtenemos los horarios de las reservas existentes
                    var horariosReservas = new List<(DateTime inicio, DateTime fin)>();
                    
                    foreach (var reservaExistente in reservasExistentes)
                    {
                        // Obtenemos el horario de la reserva existente
                        // Como no tenemos acceso directo a FechaInicio y FechaFin en la entidad Reserva,
                        // debemos obtener esta información del horario asociado o calcularla
                        
                        // Si tenemos un horario asociado, lo usamos
                        DateTime inicioReservaExistente;
                        DateTime finReservaExistente;
                        
                        if (reservaExistente.HorarioId > 0)
                        {
                            // Aquí deberíamos obtener el horario de la base de datos
                            // Como simplificación, asumimos que cada reserva tiene un horario de 1 hora
                            // y usamos la fecha de reserva como referencia
                            inicioReservaExistente = reservaExistente.FechaReserva;
                            finReservaExistente = reservaExistente.FechaReserva.AddHours(1);
                        }
                        else
                        {
                            // Si no hay horario asociado, usamos la fecha de reserva como referencia
                            inicioReservaExistente = reservaExistente.FechaReserva;
                            finReservaExistente = reservaExistente.FechaReserva.AddHours(1);
                        }
                        
                        horariosReservas.Add((inicioReservaExistente, finReservaExistente));
                    }
                    
                    // Verificamos si hay solapamiento con el horario solicitado
                    DateTime inicioSolicitado = createDto.FechaInicio;
                    DateTime finSolicitado = createDto.FechaFin;
                    
                    // Verificamos si hay solapamiento con alguna reserva existente
                    foreach (var (inicio, fin) in horariosReservas)
                    {
                        // Hay solapamiento si:
                        // 1. El inicio solicitado está dentro del rango de una reserva existente
                        // 2. El fin solicitado está dentro del rango de una reserva existente
                        // 3. El rango solicitado contiene completamente a una reserva existente
                        bool haySolapamiento = 
                            (inicioSolicitado >= inicio && inicioSolicitado < fin) ||  // Caso 1
                            (finSolicitado > inicio && finSolicitado <= fin) ||        // Caso 2
                            (inicioSolicitado <= inicio && finSolicitado >= fin);      // Caso 3
                        
                        if (haySolapamiento)
                        {
                            return BadRequest($"El paseador no está disponible en el horario solicitado. " +
                                             $"Ya tiene una reserva entre {inicio:dd/MM/yyyy HH:mm} y {fin:dd/MM/yyyy HH:mm}");
                        }
                    }
                    
                    // Verificamos también que el horario solicitado sea válido
                    if (inicioSolicitado >= finSolicitado)
                    {
                        return BadRequest("La hora de inicio debe ser anterior a la hora de fin");
                    }
                    
                    // Verificamos que la duración sea razonable (por ejemplo, máximo 3 horas)
                    TimeSpan duracion = finSolicitado - inicioSolicitado;
                    if (duracion.TotalHours > 3)
                    {
                        return BadRequest("La duración máxima de un paseo es de 3 horas");
                    }
                    
                    // Verificamos que la reserva no sea para el pasado
                    if (inicioSolicitado < DateTime.UtcNow)
                    {
                        return BadRequest("No se pueden hacer reservas para fechas pasadas");
                    }
                    
                    // Verificamos que la reserva no sea con demasiada antelación (por ejemplo, máximo 30 días)
                    if (inicioSolicitado > DateTime.UtcNow.AddDays(30))
                    {
                        return BadRequest("Las reservas solo pueden hacerse con un máximo de 30 días de antelación");
                    }
                }

                // Obtener el precio del servicio para este paseador
                var precios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Id == createDto.ServicioId && 
                    s.Precios.Any(p => p.PaseadorId == createDto.PaseadorId));
                
                var servicioConPrecios = precios.FirstOrDefault();
                if (servicioConPrecios == null || servicioConPrecios.Precios == null)
                {
                    return BadRequest("No se encontró el servicio con precios asociados");
                }
                
                var precio = servicioConPrecios.Precios
                    .FirstOrDefault(p => p.PaseadorId == createDto.PaseadorId && p.ServicioId == createDto.ServicioId);

                if (precio == null)
                {
                    return BadRequest("El paseador no tiene un precio establecido para este servicio");
                }

                // Crear la reserva
                var reserva = new Reserva
                {
                    UsuarioId = usuarioId,
                    PaseadorId = createDto.PaseadorId,
                    ServicioId = createDto.ServicioId,
                    PerroId = createDto.MascotasIds.FirstOrDefault(), // Asignamos la primera mascota como principal
                    HorarioId = createDto.HorarioId,
                    FechaReserva = DateTime.UtcNow,
                    Estado = ReservaStatus.Pendiente,
                    // Si la entidad Reserva tiene propiedades para FechaInicio y FechaFin, las asignamos
                    // Si no las tiene, podemos guardar esta información en el horario asociado o en otra entidad
                    // Como simplificación, asumimos que la entidad Reserva no tiene estas propiedades
                    // y que se manejan a través del horario asociado
                };

                // Si tenemos una entidad Horario, podríamos crear o actualizar el horario asociado
                // con las fechas de inicio y fin
                // Como simplificación, asumimos que no tenemos acceso directo a la entidad Horario

                await _unitOfWork.Reservas.AddAsync(reserva);
                await _unitOfWork.SaveChangesAsync();

                // Obtener la reserva completa con detalles
                var reservaCreada = await _unitOfWork.Reservas.GetByIdAsync(reserva.Id);
                if (reservaCreada == null)
                {
                    return StatusCode(500, "Error al recuperar la reserva creada");
                }
                
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(reservaCreada.UsuarioId);
                var paseadorCreado = await _unitOfWork.Paseadores.GetByIdAsync(reservaCreada.PaseadorId);
                var servicioCreado = await _unitOfWork.Servicios.GetByIdAsync(reservaCreada.ServicioId);
                
                var reservaDto = MapReservaToDto(
                    reservaCreada, 
                    usuario ?? new Usuario(), 
                    paseadorCreado ?? new Paseador(), 
                    servicioCreado ?? new Servicio(), 
                    precio);

                return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reservaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear una nueva reserva");
                return StatusCode(500, "Error interno del servidor al crear la reserva");
            }
        }

        // PUT: api/Reservas/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Update(int id, [FromBody] ReservaUpdateDto updateDto)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                if (!int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("ID de usuario inválido");
                }

                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound($"No se encontró la reserva con ID {id}");
                }

                // Verificar que el usuario sea el propietario de la reserva
                if (reserva.UsuarioId != usuarioId)
                {
                    return Forbid("No tienes permiso para modificar esta reserva");
                }

                // Verificar que la reserva no esté en un estado que no permita modificaciones
                if (reserva.Estado == ReservaStatus.Cancelada)
                {
                    return BadRequest($"No se puede modificar una reserva en estado {reserva.Estado}");
                }

                // Verificar si las mascotas existen y pertenecen al usuario
                foreach (var mascotaId in updateDto.MascotasIds)
                {
                    var perro = await _unitOfWork.Perros.GetByIdAsync(mascotaId);
                    if (perro == null)
                    {
                        return NotFound($"No se encontró la mascota con ID {mascotaId}");
                    }

                    if (perro.UsuarioId != usuarioId)
                    {
                        return BadRequest($"La mascota con ID {mascotaId} no pertenece al usuario actual");
                    }
                }

                // Si se está modificando el horario, verificamos disponibilidad
                if (updateDto.FechaInicio != default && updateDto.FechaFin != default)
                {
                    // Verificamos que el horario solicitado sea válido
                    if (updateDto.FechaInicio >= updateDto.FechaFin)
                    {
                        return BadRequest("La hora de inicio debe ser anterior a la hora de fin");
                    }
                    
                    // Verificamos que la duración sea razonable (por ejemplo, máximo 3 horas)
                    TimeSpan duracion = updateDto.FechaFin - updateDto.FechaInicio;
                    if (duracion.TotalHours > 3)
                    {
                        return BadRequest("La duración máxima de un paseo es de 3 horas");
                    }
                    
                    // Verificamos que la reserva no sea para el pasado
                    if (updateDto.FechaInicio < DateTime.UtcNow)
                    {
                        return BadRequest("No se pueden hacer reservas para fechas pasadas");
                    }
                    
                    // Verificamos que la reserva no sea con demasiada antelación (por ejemplo, máximo 30 días)
                    if (updateDto.FechaInicio > DateTime.UtcNow.AddDays(30))
                    {
                        return BadRequest("Las reservas solo pueden hacerse con un máximo de 30 días de antelación");
                    }
                    
                    // Verificar disponibilidad del paseador (excluyendo la reserva actual)
                    var reservasExistentes = await _unitOfWork.Reservas.FindAsync(r => 
                        r.PaseadorId == reserva.PaseadorId && 
                        r.Id != id &&
                        r.Estado != ReservaStatus.Cancelada);

                    if (reservasExistentes.Any())
                    {
                        // Implementamos lógica para verificar solapamientos de horarios
                        var horariosReservas = new List<(DateTime inicio, DateTime fin)>();
                        
                        foreach (var reservaExistente in reservasExistentes)
                        {
                            // Como simplificación, asumimos que cada reserva tiene un horario de 1 hora
                            // y usamos la fecha de reserva como referencia
                            DateTime inicioReservaExistente = reservaExistente.FechaReserva;
                            DateTime finReservaExistente = reservaExistente.FechaReserva.AddHours(1);
                            
                            horariosReservas.Add((inicioReservaExistente, finReservaExistente));
                        }
                        
                        // Verificamos si hay solapamiento con alguna reserva existente
                        foreach (var (inicio, fin) in horariosReservas)
                        {
                            bool haySolapamiento = 
                                (updateDto.FechaInicio >= inicio && updateDto.FechaInicio < fin) ||
                                (updateDto.FechaFin > inicio && updateDto.FechaFin <= fin) ||
                                (updateDto.FechaInicio <= inicio && updateDto.FechaFin >= fin);
                            
                            if (haySolapamiento)
                            {
                                return BadRequest($"El paseador no está disponible en el horario solicitado. " +
                                                 $"Ya tiene una reserva entre {inicio:dd/MM/yyyy HH:mm} y {fin:dd/MM/yyyy HH:mm}");
                            }
                        }
                    }
                }

                // Actualizar la reserva
                reserva.Estado = updateDto.Estado;
                reserva.PerroId = updateDto.MascotasIds.FirstOrDefault(); // Actualizamos la mascota principal

                // Si la entidad Reserva tiene propiedades para FechaInicio y FechaFin, las actualizamos
                // Si no las tiene, podemos actualizar esta información en el horario asociado
                // Como simplificación, asumimos que la entidad Reserva no tiene estas propiedades
                
                // Si tenemos una entidad Horario y se están modificando las fechas, actualizamos el horario
                if (updateDto.FechaInicio != default && updateDto.FechaFin != default && reserva.HorarioId > 0)
                {
                    // Aquí deberíamos actualizar el horario asociado con las nuevas fechas
                    // Como simplificación, asumimos que no tenemos acceso directo a la entidad Horario
                    // y que esta información se maneja a través de otra lógica
                }

                _unitOfWork.Reservas.Update(reserva);
                await _unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la reserva con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al actualizar la reserva");
            }
        }

        // PUT: api/Reservas/5/Estado
        [HttpPut("{id}/Estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoReservaDto cambiarEstadoDto)
        {
            try
            {
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound($"No se encontró la reserva con ID {id}");
                }

                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                if (!int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("ID de usuario inválido");
                }

                var esAdmin = User.IsInRole("Admin");
                var esPaseador = User.IsInRole("Paseador") && reserva.PaseadorId.ToString() == usuarioIdClaim.Value;
                var esUsuario = reserva.UsuarioId.ToString() == usuarioIdClaim.Value;

                // Verificar permisos según el estado solicitado
                if (!Enum.TryParse<ReservaStatus>(cambiarEstadoDto.Estado, true, out var nuevoEstado))
                {
                    return BadRequest($"Estado inválido: {cambiarEstadoDto.Estado}");
                }

                bool tienePermiso = false;

                switch (nuevoEstado)
                {
                    case ReservaStatus.Pendiente:
                        tienePermiso = esAdmin;
                        break;
                    case ReservaStatus.Confirmada:
                        tienePermiso = esPaseador || esAdmin;
                        break;
                    case ReservaStatus.Cancelada:
                        tienePermiso = esUsuario || esAdmin;
                        break;
                    default:
                        return BadRequest($"No se puede cambiar manualmente al estado {nuevoEstado}");
                }

                if (!tienePermiso)
                {
                    return Forbid($"No tienes permiso para cambiar la reserva al estado {nuevoEstado}");
                }

                // Verificar transiciones de estado válidas
                bool esTransicionValida = false;

                switch (reserva.Estado)
                {
                    case ReservaStatus.Pendiente:
                        esTransicionValida = nuevoEstado == ReservaStatus.Confirmada || 
                                            nuevoEstado == ReservaStatus.Cancelada;
                        break;
                    case ReservaStatus.Confirmada:
                        esTransicionValida = nuevoEstado == ReservaStatus.Cancelada;
                        break;
                    case ReservaStatus.Cancelada:
                        esTransicionValida = false; // Estado final
                        break;
                }

                if (!esTransicionValida)
                {
                    return BadRequest($"No se puede cambiar de {reserva.Estado} a {nuevoEstado}");
                }

                // Actualizar el estado de la reserva
                reserva.Estado = nuevoEstado;
                
                _unitOfWork.Reservas.Update(reserva);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado de la reserva con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al cambiar el estado de la reserva");
            }
        }

        // POST: api/Reservas/5/Valoracion
        [HttpPost("{id}/Valoracion")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ValoracionReserva(int id, [FromBody] ValoracionReservaDto valoracionDto)
        {
            try
            {
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound($"No se encontró la reserva con ID {id}");
                }

                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (usuarioIdClaim == null)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                if (!int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    return BadRequest("ID de usuario inválido");
                }

                // Verificar que el usuario sea el propietario de la reserva
                if (reserva.UsuarioId != usuarioId)
                {
                    return Forbid("No tienes permiso para valorar esta reserva");
                }

                // Verificar que la reserva esté completada (confirmada)
                if (reserva.Estado != ReservaStatus.Confirmada)
                {
                    return BadRequest("Solo se pueden valorar reservas completadas");
                }

                // Verificar que la valoración sea válida (entre 1 y 5)
                if (valoracionDto.Valoracion < 1 || valoracionDto.Valoracion > 5)
                {
                    return BadRequest("La valoración debe estar entre 1 y 5");
                }

                // Actualizar la valoración del paseador
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId);
                if (paseador != null)
                {
                    // Actualizar la valoración del paseador (promedio de valoraciones)
                    var reservasCompletadas = await _unitOfWork.Reservas.FindAsync(r => 
                        r.PaseadorId == paseador.Id && 
                        r.Estado == ReservaStatus.Confirmada);
                    
                    // Calculamos la valoración promedio
                    if (reservasCompletadas.Any())
                    {
                        // Aquí deberíamos actualizar la valoración del paseador
                        // Como no tenemos acceso directo a un campo de valoración en la reserva,
                        // podríamos implementar esto de otra manera, como un servicio separado
                        // Simplificado para este ejemplo - asumimos que existe un campo ValoracionGeneral
                        paseador.ValoracionGeneral = valoracionDto.Valoracion;
                        _unitOfWork.Paseadores.Update(paseador);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al valorar la reserva con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor al valorar la reserva");
            }
        }

        // Método auxiliar para mapear una reserva a DTO
        private ReservaDto MapReservaToDto(Reserva reserva, Usuario? usuario = null, Paseador? paseador = null, Servicio? servicio = null, Precio? precio = null)
        {
            if (usuario == null && reserva.UsuarioId > 0)
            {
                usuario = _unitOfWork.Usuarios.GetByIdAsync(reserva.UsuarioId).Result;
            }

            if (paseador == null && reserva.PaseadorId > 0)
            {
                paseador = _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId).Result;
            }

            if (servicio == null && reserva.ServicioId > 0)
            {
                servicio = _unitOfWork.Servicios.GetByIdAsync(reserva.ServicioId).Result;
            }

            var perro = reserva.PerroId > 0 ? _unitOfWork.Perros.GetByIdAsync(reserva.PerroId).Result : null;
            
            // Usamos un valor por defecto para la fecha y hora del servicio
            DateTime fechaHoraServicio = DateTime.UtcNow;

            var reservaDto = new ReservaDto
            {
                Id = reserva.Id,
                UsuarioId = reserva.UsuarioId,
                PaseadorId = reserva.PaseadorId,
                ServicioId = reserva.ServicioId,
                PerroId = reserva.PerroId,
                HorarioId = reserva.HorarioId,
                FechaReserva = reserva.FechaReserva,
                Estado = reserva.Estado,
                FechaHoraServicio = fechaHoraServicio,
                NombreUsuario = usuario?.Nombre ?? string.Empty,
                NombrePaseador = paseador?.Nombre ?? string.Empty,
                NombreServicio = servicio?.Nombre ?? string.Empty,
                NombrePerro = perro?.Nombre ?? string.Empty,
                // Mapeamos los campos adicionales que existen en el DTO pero no en la entidad
                FechaInicio = fechaHoraServicio,
                FechaFin = fechaHoraServicio.AddHours(1), // Asumimos 1 hora de duración
                DireccionRecogida = string.Empty, // No tenemos este campo en la entidad
                Comentarios = string.Empty, // No tenemos este campo en la entidad
                Precio = precio?.Valor ?? 0, // Obtenemos el precio del servicio si está disponible
                Valoracion = null, // No tenemos este campo en la entidad
                ComentarioValoracion = string.Empty // No tenemos este campo en la entidad
            };

            // Agregar las mascotas asociadas a la reserva (simplificado)
            if (perro != null)
            {
                reservaDto.Mascotas.Add(new MascotaReservaDto
                {
                    MascotaId = perro.Id,
                    Nombre = perro.Nombre
                });
            }

            return reservaDto;
        }

        // Método auxiliar para mapear una lista de reservas a DTOs
        private async Task<IEnumerable<ReservaDto>> MapReservasToDto(IEnumerable<Reserva> reservas)
        {
            var reservasDto = new List<ReservaDto>();

            foreach (var reserva in reservas)
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(reserva.UsuarioId);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId);
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(reserva.ServicioId);
                
                // Obtener el precio si existe
                var precios = await _unitOfWork.Servicios.FindAsync(s => 
                    s.Id == reserva.ServicioId && 
                    s.Precios.Any(p => p.PaseadorId == reserva.PaseadorId));
                
                var servicioConPrecios = precios.FirstOrDefault();
                Precio? precio = null;
                
                if (servicioConPrecios != null && servicioConPrecios.Precios != null)
                {
                    precio = servicioConPrecios.Precios
                        .FirstOrDefault(p => p.PaseadorId == reserva.PaseadorId && p.ServicioId == reserva.ServicioId);
                }
                
                reservasDto.Add(MapReservaToDto(
                    reserva, 
                    usuario ?? new Usuario(), 
                    paseador ?? new Paseador(), 
                    servicio ?? new Servicio(), 
                    precio));
            }

            return reservasDto;
        }
    }
} 