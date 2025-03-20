using DogWalk_Application.DTOs;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Repositories;
using DogWalk_Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login/usuario")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> LoginUsuario(UsuarioLoginDto loginDto)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(loginDto.Email);

                if (usuario == null)
                {
                    return Unauthorized("Email o contraseña incorrectos");
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, usuario.Password))
                {
                    return Unauthorized("Email o contraseña incorrectos");
                }

                // Generar token JWT
                var token = GenerateJwtToken(usuario.Id.ToString(), usuario.Email.Value, usuario.Rol.Nombre);
                var refreshToken = GenerateRefreshToken();

                // Aquí se podría guardar el refreshToken en la base de datos asociado al usuario

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"] ?? "60")),
                    UserId = usuario.Id,
                    UserName = $"{usuario.Nombre} {usuario.Apellido}",
                    Email = usuario.Email.Value,
                    Role = usuario.Rol.Nombre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el login de usuario: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("login/paseador")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> LoginPaseador(PaseadorLoginDto loginDto)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByEmailAsync(loginDto.Email);

                if (paseador == null)
                {
                    return Unauthorized("Email o contraseña incorrectos");
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, paseador.Password))
                {
                    return Unauthorized("Email o contraseña incorrectos");
                }

                // Generar token JWT
                var token = GenerateJwtToken(paseador.Id.ToString(), paseador.Email.Value, "Paseador");
                var refreshToken = GenerateRefreshToken();

                // Aquí se podría guardar el refreshToken en la base de datos asociado al paseador

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"] ?? "60")),
                    UserId = paseador.Id,
                    UserName = $"{paseador.Nombre} {paseador.Apellido}",
                    Email = paseador.Email.Value,
                    Role = "Paseador"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el login de paseador: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthResponseDto> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // Validar el token actual
                var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
                if (principal == null)
                {
                    return BadRequest("Token inválido o expirado");
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
                var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
                {
                    return BadRequest("Token inválido");
                }

                // Aquí se debería validar el refreshToken contra el almacenado en la base de datos
                // Por simplicidad, generamos un nuevo token sin validar el refreshToken

                var newToken = GenerateJwtToken(userId, userEmail, userRole);
                var newRefreshToken = GenerateRefreshToken();

                // Aquí se debería actualizar el refreshToken en la base de datos

                return Ok(new AuthResponseDto
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"] ?? "60")),
                    UserId = int.Parse(userId),
                    Email = userEmail,
                    Role = userRole
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar el token: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Registro(UsuarioCreateDto usuarioDto)
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

                // Generar token JWT y refresh token
                var token = GenerateJwtToken(usuario.Id.ToString(), usuario.Email.Value, "Cliente");
                var refreshToken = GenerateRefreshToken();

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"] ?? "60")),
                    UserId = usuario.Id,
                    UserName = $"{usuario.Nombre} {usuario.Apellido}",
                    Email = usuario.Email.Value,
                    Role = "Cliente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private string GenerateJwtToken(string userId, string email, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"] ?? "DogWalk_SuperSecretKey_12345678901234567890"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"] ?? "60"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? "DogWalkAPI",
                audience: _configuration["JwtSettings:Audience"] ?? "DogWalkClient",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"] ?? "DogWalk_SuperSecretKey_12345678901234567890")),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
} 