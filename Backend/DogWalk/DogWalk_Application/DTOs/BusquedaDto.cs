namespace DogWalk_Application.DTOs
{
    public class BusquedaPaseadorDto
    {
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public double? DistanciaMaxima { get; set; }
        public double? ValoracionMinima { get; set; }
        public int? ServicioId { get; set; }
    }

    public class BusquedaPaseadorCercanoDto
    {
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public double? DistanciaMaxima { get; set; }
    }

    public class BusquedaServicioDto
    {
        public string? Nombre { get; set; }
    }
} 