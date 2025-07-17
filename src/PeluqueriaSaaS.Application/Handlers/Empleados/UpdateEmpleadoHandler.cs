using MediatR;
using PeluqueriaSaaS.Application.Commands.Empleados;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Handlers.Empleados
{
    public class UpdateEmpleadoHandler : IRequestHandler<UpdateEmpleadoCommand, Empleado>
    {
        private readonly IEmpleadoRepository _repository;

        public UpdateEmpleadoHandler(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Empleado> Handle(UpdateEmpleadoCommand request, CancellationToken cancellationToken)
        {
            var empleado = await _repository.GetByIdAsync(request.Id);
            if (empleado == null)
                throw new ArgumentException($"Empleado con ID {request.Id} no encontrado");

            empleado.Nombre = request.Nombre;
            empleado.Apellido = request.Apellido;
            empleado.Email = request.Email;
            empleado.Telefono = request.Telefono;
            empleado.FechaNacimiento = request.FechaNacimiento;
            empleado.Cargo = request.Cargo;
            empleado.Salario = request.Salario;
            empleado.FechaContratacion = request.FechaContratacion;
            empleado.Horario = request.Horario;
            empleado.Direccion = request.Direccion;
            empleado.Ciudad = request.Ciudad;
            empleado.CodigoPostal = request.CodigoPostal;
            empleado.Notas = request.Notas;
            empleado.EsActivo = request.EsActivo;

            return await _repository.UpdateAsync(empleado);
        }
    }
}
