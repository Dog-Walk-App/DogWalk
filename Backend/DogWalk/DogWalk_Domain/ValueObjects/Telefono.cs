using System.Text.RegularExpressions;

namespace DogWalk_Domain.ValueObjects
{
    public class Telefono
    {
        public string Value { get; private set; }

        private Telefono(string value)
        {
            Value = value;
        }

        public static Telefono Create(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ArgumentException("El teléfono no puede estar vacío", nameof(telefono));

            // Validar formato: 9 dígitos
            var regex = new Regex(@"^[0-9]{9}$");
            if (!regex.IsMatch(telefono))
                throw new ArgumentException("El formato del teléfono no es válido. Debe ser 9 dígitos", nameof(telefono));

            return new Telefono(telefono);
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Telefono telefono)
                return false;

            return Value == telefono.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
} 