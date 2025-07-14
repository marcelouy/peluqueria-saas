// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/DeleteClienteHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class DeleteClienteHandler : IRequestHandler<DeleteClienteCommand, bool>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public DeleteClienteHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<bool> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _repositoryManager.GetClienteByIdAsync(request.Id);
            
            if (cliente == null)
                return false;

            await _repositoryManager.DeleteClienteAsync(request.Id);
            return true;
        }
    }
}
