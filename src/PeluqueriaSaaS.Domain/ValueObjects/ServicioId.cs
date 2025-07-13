namespace PeluqueriaSaaS.Domain.ValueObjects
{
    public record ServicioId(Guid Value)
    {
        public static implicit operator Guid(ServicioId id) => id.Value;
        public static implicit operator ServicioId(Guid value) => new(value);
        
        public static ServicioId New() => new(Guid.NewGuid());
        
        public override string ToString() => Value.ToString();
    }
}
