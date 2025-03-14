namespace DogWalk_Application.DTOs
{
    public class PrecioDto
    {
        public int Id { get; set; }
        public int PaseadorId { get; set; }
        public string? NombrePaseador { get; set; }
        public int ServicioId { get; set; }
        public string? NombreServicio { get; set; }
        public decimal Valor { get; set; }
    }

    public class PrecioCreateDto
    {
        public int ServicioId { get; set; }
        public decimal Valor { get; set; }
    }

    public class PrecioUpdateDto
    {
        public decimal Valor { get; set; }
    }

    public class AsignarPrecioDto
    {
        public int ServicioId { get; set; }
        public decimal Precio { get; set; }
    }
} 