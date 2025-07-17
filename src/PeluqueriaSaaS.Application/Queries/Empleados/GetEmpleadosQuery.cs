using MediatR;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Queries.Empleados
{
    public class GetEmpleadosQuery : IRequest<IEnumerable<Empleado>>
    {
        public bool? SoloActivos { get; set; }
    }
}
