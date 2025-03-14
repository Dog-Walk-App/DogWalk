namespace DogWalk_Domain.ValueObjects
{
    public class Coordenadas
    {
        public double Latitud { get; private set; }
        public double Longitud { get; private set; }

        private Coordenadas(double latitud, double longitud)
        {
            Latitud = latitud;
            Longitud = longitud;
        }

        public static Coordenadas Create(double latitud, double longitud)
        {
            // Validar rango de latitud (-90 a 90)
            if (latitud < -90 || latitud > 90)
                throw new ArgumentException("La latitud debe estar entre -90 y 90 grados", nameof(latitud));

            // Validar rango de longitud (-180 a 180)
            if (longitud < -180 || longitud > 180)
                throw new ArgumentException("La longitud debe estar entre -180 y 180 grados", nameof(longitud));

            return new Coordenadas(latitud, longitud);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Coordenadas coordenadas)
                return false;

            return Latitud == coordenadas.Latitud && Longitud == coordenadas.Longitud;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitud, Longitud);
        }

        public override string ToString()
        {
            return $"({Latitud}, {Longitud})";
        }
    }
} 