using AutoMapper;
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class ObtenerClientesHandler : IRequestHandler<ObtenerClientesQuery, IEnumerable<ClienteDto>>
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public ObtenerClientesHandler(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
        {
            var clientes = await _repository.Cliente.GetAllAsync();
            
            if (request.SoloActivos)
                clientes = clientes.Where(c => c.EsActivo);

            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }
    }
}
