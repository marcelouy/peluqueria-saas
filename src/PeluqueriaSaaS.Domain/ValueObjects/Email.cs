using System.Text.RegularExpressions;

namespace PeluqueriaSaaS.Domain.ValueObjects
{
    public class Email
    {
        public string Valor { get; private set; } = string.Empty;

        private Email() { }

        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío");

            if (!EsEmailValido(email))
                throw new ArgumentException("El formato del email no es válido");

            Valor = email.ToLowerInvariant();
        }

        private static bool EsEmailValido(string email)
        {
            const string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patron);
        }

        public static implicit operator string(Email email) => email.Valor;
        public static implicit operator Email(string email) => new(email);

        public override string ToString() => Valor;
        public override bool Equals(object? obj) => obj is Email other && Valor == other.Valor;
        public override int GetHashCode() => Valor.GetHashCode();
    }
}
