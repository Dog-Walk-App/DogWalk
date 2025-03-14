namespace DogWalk_Application.DTOs
{
    public class PrecioDto
    {
        public int PaseadorId { get; set; }
        public int ServicioId { get; set; }
        public decimal Valor { get; set; }
        public string NombrePaseador { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
    }

    public class PrecioCreateDto
    {
        public int PaseadorId { get; set; }
        public int ServicioId { get; set; }
        public decimal Valor { get; set; }
    }

    public class PrecioUpdateDto
    {
        public decimal Valor { get; set; }
    }
} 