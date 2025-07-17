using MediatR;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Handlers.Empleados
{
    public class GetEmpleadosQuery : IRequest<IEnumerable<Empleado>>
    {
        public bool SoloActivos { get; set; } = true;
    }

    public class GetEmpleadosHandler : IRequestHandler<GetEmpleadosQuery, IEnumerable<Empleado>>
    {
        private readonly IEmpleadoRepository _empleadoRepository;

        public GetEmpleadosHandler(IEmpleadoRepository empleadoRepository)
        {
            _empleadoRepository = empleadoRepository;
        }

        public async Task<IEnumerable<Empleado>> Handle(GetEmpleadosQuery request, CancellationToken cancellationToken)
        {
            if (request.SoloActivos)
            {
                return await _empleadoRepository.GetActivosAsync();
            }
            else
            {
                return await _empleadoRepository.GetAllAsync();
            }
        }
    }
}
