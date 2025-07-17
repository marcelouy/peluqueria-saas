using MediatR;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Handlers.Empleados
{
    public class CreateEmpleadoCommand : IRequest<Empleado>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Cargo { get; set; }
        public decimal? Salario { get; set; }
        public bool EsActivo { get; set; } = true;
    }

    public class CreateEmpleadoHandler : IRequestHandler<CreateEmpleadoCommand, Empleado>
    {
        private readonly IEmpleadoRepository _empleadoRepository;

        public CreateEmpleadoHandler(IEmpleadoRepository empleadoRepository)
        {
            _empleadoRepository = empleadoRepository;
        }

        public async Task<Empleado> Handle(CreateEmpleadoCommand request, CancellationToken cancellationToken)
        {
            var empleado = new Empleado
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono,
                Cargo = request.Cargo,
                Salario = request.Salario,
                EsActivo = request.EsActivo,
                FechaContratacion = DateTime.Now
            };

            return await _empleadoRepository.AddAsync(empleado);
        }
    }
}
