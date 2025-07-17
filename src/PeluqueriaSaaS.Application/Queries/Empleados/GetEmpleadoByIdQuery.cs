using MediatR;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Queries.Empleados
{
    public class GetEmpleadoByIdQuery : IRequest<Empleado?>
    {
        public int Id { get; set; }
    }
}
