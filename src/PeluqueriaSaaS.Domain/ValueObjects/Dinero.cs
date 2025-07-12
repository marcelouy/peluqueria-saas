namespace PeluqueriaSaaS.Domain.ValueObjects
{
    public class Dinero
    {
        public decimal Valor { get; private set; }
        public string Moneda { get; private set; }

        private Dinero() { Moneda = "CLP"; }

        public Dinero(decimal valor, string moneda = "CLP")
        {
            if (valor < 0) throw new ArgumentException("El valor no puede ser negativo");
            if (string.IsNullOrWhiteSpace(moneda)) throw new ArgumentException("La moneda es requerida");
            
            Valor = valor;
            Moneda = moneda.ToUpper();
        }

        public static Dinero Cero => new(0);
        public static Dinero operator +(Dinero a, Dinero b) => new(a.Valor + b.Valor, a.Moneda);
        public static Dinero operator -(Dinero a, Dinero b) => new(a.Valor - b.Valor, a.Moneda);
        public static Dinero operator *(Dinero dinero, decimal factor) => new(dinero.Valor * factor, dinero.Moneda);

        public override string ToString() => $"{Valor:N0} {Moneda}";
        public override bool Equals(object? obj) => obj is Dinero other && Valor == other.Valor && Moneda == other.Moneda;
        public override int GetHashCode() => HashCode.Combine(Valor, Moneda);
    }
}
