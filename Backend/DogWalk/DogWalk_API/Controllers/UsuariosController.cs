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
    public class UsuariosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IUnitOfWork unitOfWork, ILogger<UsuariosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
        {
            try
            {
                var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
                var usuariosDto = usuarios.Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    RolId = u.RolId,
                    Dni = u.Dni.Value,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Direccion = u.Direccion,
                    Email = u.Email.Value,
                    Telefono = u.Telefono.Value,
                    FotoPerfil = u.FotoPerfil,
                    NombreRol = u.Rol.Nombre
                });

                return Ok(usuariosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UsuarioDto>> GetById(int id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                // Verificar si el usuario actual tiene permiso para ver este usuario
                if (!User.IsInRole("Admin") && User.FindFirst(ClaimTypes.NameIdentifier)?.Value != id.ToString())
                {
                    return Forbid();
                }

                var usuarioDto = new UsuarioDto
                {
                    Id = usuario.Id,
                    RolId = usuario.RolId,
                    Dni = usuario.Dni.Value,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Direccion = usuario.Direccion,
                    Email = usuario.Email.Value,
                    Telefono = usuario.Telefono.Value,
                    FotoPerfil = usuario.FotoPerfil,
                    NombreRol = usuario.Rol.Nombre
                };

                return Ok(usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioDto>> Create(UsuarioCreateDto usuarioDto)
        {
            try
            {
                // Verificar si ya existe un usuario con el mismo email
                var existingUsuario = await _unitOfWork.Usuarios.GetByEmailAsync(usuarioDto.Email);
                if (existingUsuario != null)
                {
                    return BadRequest("Ya existe un usuario con este email");
                }

                // Verificar si ya existe un usuario con el mismo DNI
                var existingUsuarios = await _unitOfWork.Usuarios.FindAsync(u => u.Dni.Value == usuarioDto.Dni);
                if (existingUsuarios.Any())
                {
                    return BadRequest("Ya existe un usuario con este DNI");
                }

                // Crear el objeto de dominio
                var email = Email.Create(usuarioDto.Email);
                var dni = Dni.Create(usuarioDto.Dni);
                var telefono = Telefono.Create(usuarioDto.Telefono);
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Password);

                var usuario = new Usuario
                {
                    RolId = 2, // Rol de Cliente por defecto
                    Dni = dni,
                    Nombre = usuarioDto.Nombre,
                    Apellido = usuarioDto.Apellido,
                    Direccion = usuarioDto.Direccion,
                    Email = email,
                    Password = hashedPassword,
                    Telefono = telefono,
                    FotoPerfil = usuarioDto.FotoPerfil
                };

                await _unitOfWork.Usuarios.AddAsync(usuario);
                await _unitOfWork.SaveChangesAsync();

                var createdUsuarioDto = new UsuarioDto
                {
                    Id = usuario.Id,
                    RolId = usuario.RolId,
                    Dni = usuario.Dni.Value,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Direccion = usuario.Direccion,
                    Email = usuario.Email.Value,
                    Telefono = usuario.Telefono.Value,
                    FotoPerfil = usuario.FotoPerfil,
                    NombreRol = "Cliente" // Asumimos que el rol 2 es Cliente
                };

                return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, createdUsuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UsuarioUpdateDto usuarioDto)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                // Verificar si el usuario actual tiene permiso para actualizar este usuario
                if (!User.IsInRole("Admin") && User.FindFirst(ClaimTypes.NameIdentifier)?.Value != id.ToString())
                {
                    return Forbid();
                }

                // Actualizar propiedades
                usuario.Nombre = usuarioDto.Nombre;
                usuario.Apellido = usuarioDto.Apellido;
                usuario.Direccion = usuarioDto.Direccion;
                usuario.Telefono = Telefono.Create(usuarioDto.Telefono);
                usuario.FotoPerfil = usuarioDto.FotoPerfil;

                _unitOfWork.Usuarios.Update(usuario);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                _unitOfWork.Usuarios.Remove(usuario);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, UsuarioChangePasswordDto passwordDto)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                // Verificar si el usuario actual tiene permiso para cambiar la contraseña
                if (!User.IsInRole("Admin") && User.FindFirst(ClaimTypes.NameIdentifier)?.Value != id.ToString())
                {
                    return Forbid();
                }

                // Verificar contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(passwordDto.OldPassword, usuario.Password))
                {
                    return BadRequest("La contraseña actual es incorrecta");
                }

                // Verificar que la nueva contraseña y la confirmación coincidan
                if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
                {
                    return BadRequest("La nueva contraseña y la confirmación no coinciden");
                }

                // Actualizar contraseña
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);

                _unitOfWork.Usuarios.Update(usuario);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña del usuario con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 