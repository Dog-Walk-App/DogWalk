using System;
using System.Collections.Generic;
using DogWalk_Domain.Enums;

namespace DogWalk_Application.DTOs
{
    public class HorarioDto
    {
        public int Id { get; set; }
        public int PaseadorId { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Disponible { get; set; }
    }

    public class HorarioCreateDto
    {
        public DateTime FechaHora { get; set; }
    }

    public class HorarioRangoCreateDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int IntervaloMinutos { get; set; } = 60; // Por defecto, intervalos de 1 hora
        public List<int> DiasSeleccionados { get; set; } = new List<int>(); // 0 = Domingo, 1 = Lunes, etc.
    }

    public class HorarioDisponibilidadDto
    {
        public bool Disponible { get; set; }
    }

    public class HorarioUpdateDto
    {
        public DateTime FechaHora { get; set; }
        public DisponibilidadStatus Disponibilidad { get; set; }
    }
} 