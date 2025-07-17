using Microsoft.AspNetCore.Mvc;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Web.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadoRepository _empleadoRepository;

        public EmpleadosController(IEmpleadoRepository empleadoRepository)
        {
            _empleadoRepository = empleadoRepository;
        }

        // GET: Empleados
        public async Task<IActionResult> Index()
        {
            try
            {
                var empleados = await _empleadoRepository.GetAllAsync();
                return View(empleados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cargando empleados: {ex.Message}";
                return View(new List<Empleado>());
            }
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID de empleado no válido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var empleado = await _empleadoRepository.GetByIdAsync(id.Value);
                if (empleado == null)
                {
                    TempData["Error"] = "Empleado no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cargando empleado: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpleadoCreateDto empleadoDto)
        {
            if (!ModelState.IsValid)
            {
                return View(empleadoDto);
            }

            try
            {
                var empleado = new Empleado
                {
                    Nombre = empleadoDto.Nombre,
                    Apellido = empleadoDto.Apellido,
                    Email = empleadoDto.Email,
                    Telefono = empleadoDto.Telefono,
                    Cargo = empleadoDto.Cargo,
                    Salario = empleadoDto.Salario,
                    EsActivo = empleadoDto.EsActivo,
                    FechaContratacion = DateTime.Now
                };

                await _empleadoRepository.AddAsync(empleado);
                TempData["Success"] = "Empleado creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creando empleado: {ex.Message}";
                return View(empleadoDto);
            }
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID de empleado no válido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var empleado = await _empleadoRepository.GetByIdAsync(id.Value);
                if (empleado == null)
                {
                    TempData["Error"] = "Empleado no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cargando empleado para editar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Empleado empleado)
        {
            if (id != empleado.Id)
            {
                TempData["Error"] = "ID de empleado no coincide";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(empleado);
            }

            try
            {
                await _empleadoRepository.UpdateAsync(empleado);
                TempData["Success"] = "Empleado actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error actualizando empleado: {ex.Message}";
                return View(empleado);
            }
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID de empleado no válido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var empleado = await _empleadoRepository.GetByIdAsync(id.Value);
                if (empleado == null)
                {
                    TempData["Error"] = "Empleado no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cargando empleado para eliminar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _empleadoRepository.DeleteAsync(id);
                if (success)
                {
                    TempData["Success"] = "Empleado eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el empleado";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error eliminando empleado: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

