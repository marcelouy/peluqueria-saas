using MediatR;
using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;

namespace PeluqueriaSaaS.Web.Controllers;

// 📝 EXPLICACIÓN: Este es nuestro Controller
// Solo tiene UNA dependencia: IMediator
// No sabe NADA sobre bases de datos, repositorios, etc.
public class ClientesController : Controller
{
    private readonly IMediator _mediator;

    // 🎯 AQUÍ PASA LA MAGIA DE INYECCIÓN DE DEPENDENCIAS
    // El framework automáticamente "inyecta" IMediator aquí
    // Tu no tienes que crear nada, llega solo
    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // 📋 ACCIÓN 1: Listar todos los clientes
    // GET: /Clientes
    public async Task<IActionResult> Index()
    {
        // 🚀 AQUÍ VES CQRS EN ACCIÓN
        // Enviamos una QUERY (consulta) via MediatR
        var query = new ObtenerClientesQuery();
        var clientes = await _mediator.Send(query);
        
        // 📊 Los datos van directo a la Vista
        return View(clientes);
    }

    // 📝 ACCIÓN 2: Mostrar formulario para crear cliente
    // GET: /Clientes/Create
    public IActionResult Create()
    {
        // 🎨 Solo muestra el formulario vacío
        return View();
    }

    // 💾 ACCIÓN 3: Procesar formulario de crear cliente
    // POST: /Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearClienteCommand command)
    {
        try
        {
            // 🎯 AQUÍ VES CQRS OTRA VEZ
            // Enviamos un COMMAND (modificación) via MediatR
            var clienteId = await _mediator.Send(command);
            
            // ✅ Si todo salió bien, redirigimos a la lista
            TempData["Success"] = $"Cliente creado exitosamente con ID: {clienteId}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // ❌ Si algo falló, mostramos error
            ModelState.AddModelError("", ex.Message);
            return View(command);
        }
    }

    // 👁️ ACCIÓN 4: Ver detalles de un cliente
    // GET: /Clientes/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        // 🔍 Consulta un cliente específico
        var query = new ObtenerClientePorIdQuery { Id = id };
        var cliente = await _mediator.Send(query);
        
        if (cliente == null)
        {
            return NotFound();
        }
        
        return View(cliente);
    }
}
