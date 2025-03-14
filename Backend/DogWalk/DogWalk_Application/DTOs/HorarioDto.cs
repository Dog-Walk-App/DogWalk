using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class HorarioDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public DisponibilidadStatus Disponibilidad { get; set; }
        public string DisponibilidadNombre => Disponibilidad.ToString();
    }

    public class HorarioCreateDto
    {
        public DateTime FechaHora { get; set; }
        public DisponibilidadStatus Disponibilidad { get; set; }
    }

    public class HorarioUpdateDto
    {
        public DateTime FechaHora { get; set; }
        public DisponibilidadStatus Disponibilidad { get; set; }
    }
} 