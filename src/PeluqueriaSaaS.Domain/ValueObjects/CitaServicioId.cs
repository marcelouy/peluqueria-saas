namespace PeluqueriaSaaS.Domain.ValueObjects
{
    public record CitaServicioId(int Value)
    {
        public static implicit operator int(CitaServicioId id) => id.Value;
        public static implicit operator CitaServicioId(int value) => new(value);
        
        public override string ToString() => Value.ToString();
    }
}
