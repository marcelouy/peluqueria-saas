namespace PeluqueriaSaaS.Domain.ValueObjects
{
    public record CitaId(Guid Value)
    {
        public static implicit operator Guid(CitaId id) => id.Value;
        public static implicit operator CitaId(Guid value) => new(value);
        
        public static CitaId New() => new(Guid.NewGuid());
        
        public override string ToString() => Value.ToString();
    }
}
