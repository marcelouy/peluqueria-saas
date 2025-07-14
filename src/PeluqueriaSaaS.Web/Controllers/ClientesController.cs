using MediatR;
using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            try
            {
                var query = new ObtenerClientesQuery();
                var clientes = await _mediator.Send(query);
                return View(clientes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar clientes: {ex.Message}";
                return View(new List<PeluqueriaSaaS.Application.DTOs.ClienteDto>());
            }
        }

        // GET: Clientes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var query = new ObtenerClientePorIdQuery(id);
                var cliente = await _mediator.Send(query);
                
                if (cliente == null)
                {
                    TempData["Error"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                
                return View(cliente);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar cliente: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View(new CrearClienteCommand());
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearClienteCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(command);
                }

                var cliente = await _mediator.Send(command);
                TempData["Success"] = "Cliente creado exitosamente";
                return RedirectToAction(nameof(Details), new { id = cliente.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear cliente: {ex.Message}";
                ModelState.AddModelError("", "Ocurrió un error al guardar el cliente");
                return View(command);
            }
        }

        // GET: Clientes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var query = new ObtenerClientePorIdQuery(id);
                var cliente = await _mediator.Send(query);
                
                if (cliente == null)
                {
                    TempData["Error"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                
                var command = new UpdateClienteCommand
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono,
                    FechaNacimiento = cliente.FechaNacimiento
                };
                
                return View(command);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar cliente: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateClienteCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(command);
                
                var cliente = await _mediator.Send(command);
                
                if (cliente == null)
                {
                    TempData["Error"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Success"] = "Cliente actualizado exitosamente";
                return RedirectToAction(nameof(Details), new { id = cliente.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar cliente: {ex.Message}";
                ModelState.AddModelError("", "Ocurrió un error al actualizar el cliente");
                return View(command);
            }
        }

        // POST: Clientes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var command = new DeleteClienteCommand(id);
                var result = await _mediator.Send(command);
                
                if (!result)
                {
                    TempData["Error"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Success"] = "Cliente eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar cliente: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
