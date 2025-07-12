using System.Text.RegularExpressions;

namespace PeluqueriaSaaS.Domain.ValueObjects
{
    public class Telefono
    {
        public string Numero { get; private set; } = string.Empty;
        public string? CodigoPais { get; private set; }

        private Telefono() { }

        public Telefono(string numero, string? codigoPais = null)
        {
            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("El número de teléfono no puede estar vacío");

            var numeroLimpio = LimpiarNumero(numero);
            if (!EsNumeroValido(numeroLimpio))
                throw new ArgumentException("El formato del teléfono no es válido");

            Numero = numeroLimpio;
            CodigoPais = codigoPais ?? "+56"; // Chile por defecto
        }

        private static string LimpiarNumero(string numero)
        {
            return Regex.Replace(numero, @"[^\d+]", "");
        }

        private static bool EsNumeroValido(string numero)
        {
            return numero.Length >= 8 && numero.Length <= 15;
        }

        public string NumeroCompleto => $"{CodigoPais} {Numero}";

        public static implicit operator string(Telefono telefono) => telefono.NumeroCompleto;
        public static implicit operator Telefono(string numero) => new(numero);

        public override string ToString() => NumeroCompleto;
        public override bool Equals(object? obj) => obj is Telefono other && Numero == other.Numero && CodigoPais == other.CodigoPais;
        public override int GetHashCode() => HashCode.Combine(Numero, CodigoPais);
    }
}
