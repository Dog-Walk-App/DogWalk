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
    [Authorize(Roles = "Usuario")]
    public class CarritosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CarritosController> _logger;

        public CarritosController(IUnitOfWork unitOfWork, ILogger<CarritosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Carritos
        [HttpGet]
        public async Task<ActionResult<CarritoDto>> GetCarrito()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                
                if (carrito == null || !carrito.Any())
                {
                    // Si no existe un carrito, crear uno nuevo
                    var nuevoCarrito = new Carrito
                    {
                        UsuarioId = usuarioId,
                        FechaCreacion = DateTime.Now
                    };
                    
                    await _unitOfWork.Carritos.AddAsync(nuevoCarrito);
                    await _unitOfWork.SaveChangesAsync();
                    
                    carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                }

                var carritoItem = carrito.FirstOrDefault();
                if (carritoItem != null)
                {
                    var items = await _unitOfWork.ItemsCarrito.GetItemsCarritoAsync(carritoItem.Id);
                    var itemsDto = new List<ItemCarritoDto>();

                    foreach (var item in items)
                    {
                        var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ArticuloId);
                        
                        itemsDto.Add(new ItemCarritoDto
                        {
                            Id = item.Id,
                            ArticuloId = item.ArticuloId,
                            Cantidad = item.Cantidad,
                            Precio = item.Precio,
                            NombreArticulo = articulo?.Nombre ?? "Artículo desconocido",
                            ImagenUrl = articulo?.ImagenPrincipal ?? string.Empty
                        });
                    }

                    var carritoDto = new CarritoDto
                    {
                        Id = carritoItem.Id,
                        UsuarioId = carritoItem.UsuarioId,
                        FechaCreacion = carritoItem.FechaCreacion,
                        Items = itemsDto,
                        Total = itemsDto.Sum(i => i.Precio * i.Cantidad)
                    };

                    return Ok(carritoDto);
                }
                else
                {
                    // Devolver un carrito vacío
                    return Ok(new CarritoDto
                    {
                        Id = 0,
                        UsuarioId = usuarioId,
                        FechaCreacion = DateTime.Now,
                        Items = new List<ItemCarritoDto>(),
                        Total = 0
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Carritos/Items
        [HttpPost("Items")]
        public async Task<ActionResult<ItemCarritoDto>> AddItemToCarrito(ItemCarritoCreateDto itemDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                
                if (carrito == null || !carrito.Any())
                {
                    // Si no existe un carrito, crear uno nuevo
                    var nuevoCarrito = new Carrito
                    {
                        UsuarioId = usuarioId,
                        FechaCreacion = DateTime.Now
                    };
                    
                    await _unitOfWork.Carritos.AddAsync(nuevoCarrito);
                    await _unitOfWork.SaveChangesAsync();
                    
                    carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                }

                var carritoItem = carrito.FirstOrDefault();
                if (carritoItem != null)
                {
                    // Verificar si el artículo ya está en el carrito
                    var itemExistente = await _unitOfWork.ItemsCarrito.GetItemCarritoByArticuloIdAsync(carritoItem.Id, itemDto.ArticuloId);
                    
                    if (itemExistente != null)
                    {
                        // Actualizar cantidad
                        itemExistente.Cantidad += itemDto.Cantidad;
                        _unitOfWork.ItemsCarrito.Update(itemExistente);
                        await _unitOfWork.SaveChangesAsync();
                        
                        return Ok(new ItemCarritoDto
                        {
                            Id = itemExistente.Id,
                            ArticuloId = itemExistente.ArticuloId,
                            Cantidad = itemExistente.Cantidad,
                            Precio = itemExistente.Precio
                        });
                    }
                    
                    // Obtener el artículo
                    var articulo = await _unitOfWork.Articulos.GetByIdAsync(itemDto.ArticuloId);
                    if (articulo == null)
                    {
                        return NotFound($"No se encontró el artículo con ID {itemDto.ArticuloId}");
                    }
                    
                    // Crear nuevo item
                    var nuevoItem = new ItemCarrito
                    {
                        CarritoId = carritoItem.Id,
                        ArticuloId = itemDto.ArticuloId,
                        Cantidad = itemDto.Cantidad,
                        Precio = articulo.Precio
                    };
                    
                    await _unitOfWork.ItemsCarrito.AddItemCarritoAsync(nuevoItem);
                    await _unitOfWork.SaveChangesAsync();
                    
                    return CreatedAtAction(nameof(GetCarrito), new { id = nuevoItem.Id }, new ItemCarritoDto
                    {
                        Id = nuevoItem.Id,
                        ArticuloId = nuevoItem.ArticuloId,
                        Cantidad = nuevoItem.Cantidad,
                        Precio = nuevoItem.Precio
                    });
                }
                else
                {
                    return NotFound("No se encontró el carrito del usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al añadir un item al carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Carritos/Items/{id}
        [HttpPut("Items/{id}")]
        public async Task<IActionResult> UpdateItemCarrito(int id, ItemCarritoUpdateDto itemDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                if (carrito == null || !carrito.Any())
                {
                    return NotFound("No se encontró el carrito del usuario");
                }

                var carritoItem = carrito.FirstOrDefault();
                if (carritoItem != null)
                {
                    var item = await _unitOfWork.ItemsCarrito.GetItemCarritoByIdAsync(id);
                    if (item == null)
                    {
                        return NotFound($"No se encontró el item con ID {id}");
                    }

                    if (item.CarritoId != carritoItem.Id)
                    {
                        return Forbid("No tienes permiso para modificar este item");
                    }

                    if (itemDto.Cantidad <= 0)
                    {
                        // Si la cantidad es 0 o negativa, eliminar el item
                        await _unitOfWork.ItemsCarrito.DeleteItemCarritoAsync(item);
                    }
                    else
                    {
                        // Actualizar la cantidad
                        item.Cantidad = itemDto.Cantidad;
                    }

                    await _unitOfWork.SaveChangesAsync();

                    return NoContent();
                }
                else
                {
                    return NotFound("No se encontró el carrito del usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar un item del carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Carritos/Items/{id}
        [HttpDelete("Items/{id}")]
        public async Task<IActionResult> DeleteItemCarrito(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                if (carrito == null || !carrito.Any())
                {
                    return NotFound("No se encontró el carrito del usuario");
                }

                var carritoItem = carrito.FirstOrDefault();
                if (carritoItem != null)
                {
                    var item = await _unitOfWork.ItemsCarrito.GetItemCarritoByIdAsync(id);
                    if (item == null)
                    {
                        return NotFound($"No se encontró el item con ID {id}");
                    }

                    if (item.CarritoId != carritoItem.Id)
                    {
                        return Forbid("No tienes permiso para eliminar este item");
                    }

                    await _unitOfWork.ItemsCarrito.DeleteItemCarritoAsync(item);
                    await _unitOfWork.SaveChangesAsync();

                    return NoContent();
                }
                else
                {
                    return NotFound("No se encontró el carrito del usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar un item del carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Carritos
        [HttpDelete]
        public async Task<IActionResult> ClearCarrito()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (usuarioId == 0)
                {
                    return Unauthorized("Usuario no autenticado correctamente");
                }

                var carrito = await _unitOfWork.Carritos.GetCarritoByUsuarioIdAsync(usuarioId);
                if (carrito == null || !carrito.Any())
                {
                    return NotFound("No se encontró el carrito del usuario");
                }

                var carritoItem = carrito.FirstOrDefault();
                if (carritoItem != null)
                {
                    await _unitOfWork.ItemsCarrito.ClearCarritoAsync(carritoItem.Id);
                    await _unitOfWork.SaveChangesAsync();

                    return NoContent();
                }
                else
                {
                    return NotFound("No se encontró el carrito del usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vaciar el carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 