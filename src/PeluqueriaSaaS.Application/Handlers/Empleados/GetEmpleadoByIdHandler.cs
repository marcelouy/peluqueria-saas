using MediatR;
using PeluqueriaSaaS.Application.Queries.Empleados;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Handlers.Empleados
{
    public class GetEmpleadoByIdHandler : IRequestHandler<GetEmpleadoByIdQuery, Empleado?>
    {
        private readonly IEmpleadoRepository _repository;

        public GetEmpleadoByIdHandler(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Empleado?> Handle(GetEmpleadoByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id);
        }
    }
}
