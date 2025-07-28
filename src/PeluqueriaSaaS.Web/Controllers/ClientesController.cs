using MediatR;
using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Application.DTOs;
using ClosedXML.Excel;

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
                return View(new List<ClienteDto>());
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
                    Notas = cliente.Notas,
                    EsActivo = cliente.EsActivo
                };
                
                return View(command);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar cliente: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportarClientesExcel()
        {
            try
            {
                Console.WriteLine($"📊 ExportarClientesExcel - Iniciando export clientes");

                // ✅ FIX: Cambiar GetClientesQuery por ObtenerClientesQuery (naming consistency)
                var request = new ObtenerClientesQuery();
                var clientesDto = await _mediator.Send(request);
                
                Console.WriteLine($"📊 Total clientes para export: {clientesDto.Count()}");

                // Crear Excel usando ClosedXML
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Clientes");

                // Configurar encabezados - WITH FECHAREGISTRO (correct DTO property)
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nombre";
                worksheet.Cell(1, 3).Value = "Apellido";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Teléfono";
                worksheet.Cell(1, 6).Value = "Fecha Nacimiento";
                worksheet.Cell(1, 7).Value = "Fecha Registro";
                worksheet.Cell(1, 8).Value = "Estado";
                worksheet.Cell(1, 9).Value = "Notas";

                // Estilo encabezados
                var headerRange = worksheet.Range(1, 1, 1, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Llenar datos - WITH FECHAREGISTRO (using correct DTO property)
                int row = 2;
                foreach (var cliente in clientesDto)
                {
                    worksheet.Cell(row, 1).Value = cliente.Id;
                    worksheet.Cell(row, 2).Value = cliente.Nombre ?? "";
                    worksheet.Cell(row, 3).Value = cliente.Apellido ?? "";
                    worksheet.Cell(row, 4).Value = cliente.Email ?? "";
                    worksheet.Cell(row, 5).Value = cliente.Telefono ?? "";
                    worksheet.Cell(row, 6).Value = cliente.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 7).Value = cliente.FechaRegistro.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 8).Value = cliente.EsActivo ? "Activo" : "Inactivo";
                    worksheet.Cell(row, 9).Value = cliente.Notas ?? "";
                    row++;
                }

                // Ajustar ancho columnas automáticamente
                worksheet.Columns().AdjustToContents();

                // Agregar filtros automáticos
                worksheet.RangeUsed().SetAutoFilter();

                // Crear nombre archivo con timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"clientes_export_{timestamp}.xlsx";

                // Crear stream y devolver archivo
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                Console.WriteLine($"✅ Excel clientes generado exitosamente: {fileName}");

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generando Excel clientes: {ex.Message}");
                TempData["Error"] = "Error al generar el archivo Excel de clientes. Intenta nuevamente.";
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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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