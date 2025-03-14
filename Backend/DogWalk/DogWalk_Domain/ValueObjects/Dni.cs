using System.Text.RegularExpressions;

namespace DogWalk_Domain.ValueObjects
{
    public class Dni
    {
        public string Value { get; private set; }

        private Dni(string value)
        {
            Value = value;
        }

        public static Dni Create(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException("El DNI no puede estar vacío", nameof(dni));

            // Validar formato: 8 dígitos seguidos de una letra mayúscula
            var regex = new Regex(@"^[0-9]{8}[A-Z]$");
            if (!regex.IsMatch(dni))
                throw new ArgumentException("El formato del DNI no es válido. Debe ser 8 dígitos seguidos de una letra mayúscula", nameof(dni));

            return new Dni(dni);
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Dni dni)
                return false;

            return Value == dni.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
} 