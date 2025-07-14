// src/PeluqueriaSaaS.Application/Features/Clientes/Commands/DeleteClienteCommand.cs
using MediatR;

namespace PeluqueriaSaaS.Application.Features.Clientes.Commands
{
    public class DeleteClienteCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteClienteCommand(int id)
        {
            Id = id;
        }
    }
}
