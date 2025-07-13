using PeluqueriaSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers;

// ?? HANDLER: Procesa la Query
// Aquí es donde se conecta con el Repository
public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, ClienteDto?>
{
    private readonly IRepositoryManagerTemp _repositoryManager;
    private readonly IMapper _mapper;

    // ?? MÁS INYECCIÓN DE DEPENDENCIAS
    // Necesita Repository y Mapper, se los inyectan automáticamente
    public ObtenerClientePorIdHandler(IRepositoryManagerTemp repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<ClienteDto?> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
    {
        // ?? USA EL REPOSITORY que ya tienes funcionando
        var cliente = await _repositoryManager.Cliente.GetByIdAsync(request.Id);
        
        if (cliente == null)
            return null;
            
        // ?? AUTOMAPPER convierte Entity a DTO
        return _mapper.Map<ClienteDto>(cliente);
    }
}



