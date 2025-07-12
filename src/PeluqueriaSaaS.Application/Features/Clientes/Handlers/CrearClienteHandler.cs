using AutoMapper;
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Entities;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, ClienteDto>
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public CrearClienteHandler(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ClienteDto> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = new Cliente(
                request.Nombre,
                request.Apellido,
                request.Email,
                request.Telefono,
                request.FechaNacimiento
            );

            await _repository.Cliente.AddAsync(cliente);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ClienteDto>(cliente);
        }
    }
}
