using MediatR;
using PeluqueriaSaaS.Application.Commands.Empleados;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Handlers.Empleados
{
    public class DeleteEmpleadoHandler : IRequestHandler<DeleteEmpleadoCommand, bool>
    {
        private readonly IEmpleadoRepository _repository;

        public DeleteEmpleadoHandler(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteEmpleadoCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeleteAsync(request.Id);
        }
    }
}
