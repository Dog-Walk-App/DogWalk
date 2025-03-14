using System.Text.RegularExpressions;

namespace DogWalk_Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; private set; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));

            // Validar formato de email
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email))
                throw new ArgumentException("El formato del email no es válido", nameof(email));

            return new Email(email);
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Email email)
                return false;

            return Value.ToLower() == email.Value.ToLower();
        }

        public override int GetHashCode()
        {
            return Value.ToLower().GetHashCode();
        }
    }
} 