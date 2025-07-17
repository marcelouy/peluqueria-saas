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

        public IActionResult Create()
        {
            return View(new CrearClienteCommand());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearClienteCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(command);

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
                    FechaNacimiento = cliente.FechaNacimiento,
                    Direccion = cliente.Direccion,
                    Ciudad = cliente.Ciudad,
                    CodigoPostal = cliente.CodigoPostal,
                    Notas = cliente.Notas,
                    EsActivo = cliente.EsActivo,
                    UltimaVisita = cliente.UltimaVisita
                };
                
                return View(command);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar cliente: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

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

        // RUTA CORREGIDA - Acepta POST con parámetro en la URL
        [HttpPost]
        [Route("Clientes/Delete/{id}")]
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
