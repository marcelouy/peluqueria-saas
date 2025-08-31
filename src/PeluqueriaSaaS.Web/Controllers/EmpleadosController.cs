using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            PrepararDropdownData();
            return View();
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpleadoCreateDto empleadoDto)
        {
            if (!ModelState.IsValid)
            {
                PrepararDropdownData();
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
                    FechaNacimiento = empleadoDto.FechaNacimiento,
                    FechaContratacion = empleadoDto.FechaContratacion,
                    Direccion = empleadoDto.Direccion,
                    Ciudad = empleadoDto.Ciudad,
                    CodigoPostal = empleadoDto.CodigoPostal,
                    Horario = empleadoDto.Horario,
                    Notas = empleadoDto.Notas ?? "",
                    EsActivo = empleadoDto.EsActivo
                };

                await _empleadoRepository.AddAsync(empleado);
                TempData["Success"] = "Empleado creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creando empleado: {ex.Message}";
                PrepararDropdownData();
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

                // ✅ FIX: Cargar opciones para dropdowns
                PrepararDropdownData(empleado.Cargo, empleado.Horario);
                
                return View(empleado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
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
                PrepararDropdownData(empleado.Cargo, empleado.Horario);
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
                PrepararDropdownData(empleado.Cargo, empleado.Horario);
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
                TempData["Error"] = $"Error cargando empleado: {ex.Message}";
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

        // ✅ MÉTODO HELPER PARA DROPDOWNS
        private void PrepararDropdownData(string cargoSeleccionado = null, string horarioSeleccionado = null)
        {
            // Cargos disponibles
            var cargos = new[]
            {
                "Estilista",
                "Estilista Senior", 
                "Colorista",
                "Barbero",
                "Manicurista",
                "Recepcionista",
                "Gerente",
                "Auxiliar",
                "Sistema"
            };

            ViewBag.CargosOptions = new SelectList(cargos, cargoSeleccionado);

            // Horarios disponibles
            var horarios = new[]
            {
                "Lunes a Viernes 9:00 - 18:00",
                "Lunes a Sábado 9:00 - 18:00",
                "Turno Mañana 8:00 - 14:00",
                "Turno Tarde 14:00 - 20:00",
                "Part-time Mañanas",
                "Part-time Tardes",
                "Flexible"
            };

            ViewBag.HorariosOptions = new SelectList(horarios, horarioSeleccionado);
        }
    }
}