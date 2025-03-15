using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using DogWalk_Application.DTOs;
using DogWalk_Domain.Repositories;
using DogWalk_Domain.Entities;
using System.Linq;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticulosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ArticulosController> _logger;

        public ArticulosController(IUnitOfWork unitOfWork, ILogger<ArticulosController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/Articulos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticuloDto>>> GetArticulos()
        {
            try
            {
                var articulos = await _unitOfWork.Articulos.GetAllAsync();
                var articulosDto = new List<ArticuloDto>();

                foreach (var articulo in articulos)
                {
                    var imagenes = await _unitOfWork.Articulos.GetImagenesArticuloAsync(articulo.Id);
                    
                    articulosDto.Add(new ArticuloDto
                    {
                        Id = articulo.Id,
                        Nombre = articulo.Nombre,
                        Descripcion = articulo.Descripcion ?? string.Empty,
                        Precio = articulo.Precio,
                        Stock = articulo.Stock,
                        Categoria = articulo.Categoria ?? string.Empty,
                        ImagenPrincipal = articulo.ImagenPrincipal ?? string.Empty,
                        Imagenes = imagenes.Select(i => i.Url).ToList()
                    });
                }

                return Ok(articulosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los artículos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Articulos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticuloDto>> GetArticulo(int id)
        {
            try
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
                if (articulo == null)
                {
                    return NotFound($"No se encontró un artículo con el ID {id}");
                }

                var imagenes = await _unitOfWork.Articulos.GetImagenesArticuloAsync(articulo.Id);
                
                var articuloDto = new ArticuloDto
                {
                    Id = articulo.Id,
                    Nombre = articulo.Nombre,
                    Descripcion = articulo.Descripcion ?? string.Empty,
                    Precio = articulo.Precio,
                    Stock = articulo.Stock,
                    Categoria = articulo.Categoria ?? string.Empty,
                    ImagenPrincipal = articulo.ImagenPrincipal ?? string.Empty,
                    Imagenes = imagenes.Select(i => i.Url).ToList()
                };

                return Ok(articuloDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el artículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Articulos/Categoria/{categoria}
        [HttpGet("Categoria/{categoria}")]
        public async Task<ActionResult<IEnumerable<ArticuloDto>>> GetArticulosByCategoria(string categoria)
        {
            try
            {
                var articulos = await _unitOfWork.Articulos.GetArticulosByCategoriaAsync(categoria);
                var articulosDto = new List<ArticuloDto>();

                foreach (var articulo in articulos)
                {
                    var imagenes = await _unitOfWork.Articulos.GetImagenesArticuloAsync(articulo.Id);
                    
                    articulosDto.Add(new ArticuloDto
                    {
                        Id = articulo.Id,
                        Nombre = articulo.Nombre,
                        Descripcion = articulo.Descripcion ?? string.Empty,
                        Precio = articulo.Precio,
                        Stock = articulo.Stock,
                        Categoria = articulo.Categoria ?? string.Empty,
                        ImagenPrincipal = articulo.ImagenPrincipal ?? string.Empty,
                        Imagenes = imagenes.Select(i => i.Url).ToList()
                    });
                }

                return Ok(articulosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los artículos por categoría");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Articulos
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArticuloDto>> CreateArticulo(ArticuloCreateDto articuloDto)
        {
            try
            {
                var articulo = new Articulo
                {
                    Nombre = articuloDto.Nombre,
                    Descripcion = articuloDto.Descripcion,
                    Precio = articuloDto.Precio,
                    Stock = articuloDto.Stock,
                    Categoria = articuloDto.Categoria,
                    ImagenPrincipal = articuloDto.ImagenPrincipal
                };

                await _unitOfWork.Articulos.AddAsync(articulo);
                await _unitOfWork.SaveChangesAsync();

                // Añadir imágenes adicionales si existen
                if (articuloDto.Imagenes != null && articuloDto.Imagenes.Count > 0)
                {
                    foreach (var url in articuloDto.Imagenes)
                    {
                        var imagen = new ImagenArticulo
                        {
                            ArticuloId = articulo.Id,
                            Url = url
                        };

                        await _unitOfWork.Articulos.AddImagenArticuloAsync(imagen);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                var createdArticuloDto = new ArticuloDto
                {
                    Id = articulo.Id,
                    Nombre = articulo.Nombre,
                    Descripcion = articulo.Descripcion ?? string.Empty,
                    Precio = articulo.Precio,
                    Stock = articulo.Stock,
                    Categoria = articulo.Categoria ?? string.Empty,
                    ImagenPrincipal = articulo.ImagenPrincipal ?? string.Empty,
                    Imagenes = articuloDto.Imagenes ?? new List<string>()
                };

                return CreatedAtAction(nameof(GetArticulo), new { id = articulo.Id }, createdArticuloDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el artículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Articulos/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateArticulo(int id, ArticuloUpdateDto articuloDto)
        {
            try
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
                if (articulo == null)
                {
                    return NotFound($"No se encontró un artículo con el ID {id}");
                }

                articulo.Nombre = articuloDto.Nombre;
                articulo.Descripcion = articuloDto.Descripcion;
                articulo.Precio = articuloDto.Precio;
                articulo.Stock = articuloDto.Stock;
                articulo.Categoria = articuloDto.Categoria;
                
                if (!string.IsNullOrEmpty(articuloDto.ImagenPrincipal))
                {
                    articulo.ImagenPrincipal = articuloDto.ImagenPrincipal;
                }

                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el artículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Articulos/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArticulo(int id)
        {
            try
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
                if (articulo == null)
                {
                    return NotFound($"No se encontró un artículo con el ID {id}");
                }

                // Verificar si el artículo está en algún carrito
                var enCarrito = await _unitOfWork.Articulos.IsArticuloInCarritoAsync(id);
                if (enCarrito)
                {
                    return BadRequest("No se puede eliminar un artículo que está en el carrito de algún usuario");
                }

                // Eliminar imágenes asociadas
                await _unitOfWork.Articulos.DeleteImagenesArticuloAsync(id);
                
                // Eliminar el artículo
                _unitOfWork.Articulos.Remove(articulo);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el artículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Articulos/{id}/Imagenes
        [HttpPost("{id}/Imagenes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<string>>> AddImagenesToArticulo(int id, [FromBody] List<string> urls)
        {
            try
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
                if (articulo == null)
                {
                    return NotFound($"No se encontró un artículo con el ID {id}");
                }

                if (urls == null || urls.Count == 0)
                {
                    return BadRequest("No se proporcionaron URLs de imágenes");
                }

                foreach (var url in urls)
                {
                    var imagen = new ImagenArticulo
                    {
                        ArticuloId = id,
                        Url = url
                    };

                    await _unitOfWork.Articulos.AddImagenArticuloAsync(imagen);
                }

                await _unitOfWork.SaveChangesAsync();

                var imagenes = await _unitOfWork.Articulos.GetImagenesArticuloAsync(id);
                var imagenesUrls = imagenes.Select(i => i.Url).ToList();

                return Ok(imagenesUrls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al añadir imágenes al artículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Articulos/{id}/Imagenes/{imagenId}
        [HttpDelete("{id}/Imagenes/{imagenId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImagenArticulo(int id, int imagenId)
        {
            try
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
                if (articulo == null)
                {
                    return NotFound($"No se encontró un artículo con el ID {id}");
                }

                var imagen = await _unitOfWork.Articulos.GetImagenArticuloByIdAsync(imagenId);
                if (imagen == null)
                {
                    return NotFound($"No se encontró una imagen con el ID {imagenId}");
                }

                if (imagen.ArticuloId != id)
                {
                    return BadRequest("La imagen no pertenece al artículo especificado");
                }

                await _unitOfWork.Articulos.DeleteImagenArticuloAsync(imagen);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la imagen del artículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
} 