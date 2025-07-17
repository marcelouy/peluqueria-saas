using MediatR;

namespace PeluqueriaSaaS.Application.Commands.Empleados
{
    public class DeleteEmpleadoCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
