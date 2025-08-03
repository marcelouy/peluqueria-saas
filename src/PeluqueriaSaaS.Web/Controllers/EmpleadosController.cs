// ✅ REEMPLAZAR COMPLETO: src/PeluqueriaSaaS.Web/Controllers/EmpleadosController.cs
// FIXED: Edit GET method con dropdown data + mejor cargo options

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
            // ✅ PREPARAR DROPDOWN DATA PARA CREATE
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
                // ✅ REPOBLAR DROPDOWN DATA EN CASO DE ERROR
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
                    Notas = empleadoDto.Notas ?? "",  // Evitar NULL como en Clientes
                    EsActivo = empleadoDto.EsActivo
                };

                await _empleadoRepository.AddAsync(empleado);
                TempData["Success"] = "Empleado creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creando empleado: {ex.Message}";
                PrepararDropdownData(); // ✅ REPOBLAR EN CASO DE ERROR
                return View(empleadoDto);
            }
        }

        // ✅ FIXED: GET: Empleados/Edit/5
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

                // ✅ CRITICAL FIX: PREPARAR DROPDOWN DATA ANTES DE MOSTRAR VIEW
                PrepararDropdownData(empleado.Cargo);

                Console.WriteLine($"🔍 DEBUG Edit: Empleado {empleado.Nombre} - Cargo: '{empleado.Cargo}'");

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
                // ✅ REPOBLAR DROPDOWN DATA EN CASO DE ERROR
                PrepararDropdownData(empleado.Cargo);
                return View(empleado);
            }

            try
            {
                Console.WriteLine($"🔍 DEBUG Save: Empleado {empleado.Nombre} - Cargo: '{empleado.Cargo}'");
                
                await _empleadoRepository.UpdateAsync(empleado);
                TempData["Success"] = "Empleado actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error actualizando empleado: {ex.Message}";
                PrepararDropdownData(empleado.Cargo);
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

        // ✅ NUEVO MÉTODO: PREPARAR DROPDOWN DATA
        private void PrepararDropdownData(string? cargoSeleccionado = null)
        {
            // ✅ CARGO OPTIONS - INCLUIR TODOS LOS VALORES POSIBLES
            var cargosOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar cargo...", Selected = string.IsNullOrEmpty(cargoSeleccionado) },
                new SelectListItem { Value = "Estilista", Text = "Estilista", Selected = cargoSeleccionado == "Estilista" },
                new SelectListItem { Value = "Estilista Senior", Text = "Estilista Senior", Selected = cargoSeleccionado == "Estilista Senior" },
                new SelectListItem { Value = "Colorista", Text = "Colorista", Selected = cargoSeleccionado == "Colorista" },
                new SelectListItem { Value = "Barbero", Text = "Barbero", Selected = cargoSeleccionado == "Barbero" },
                new SelectListItem { Value = "Manicurista", Text = "Manicurista", Selected = cargoSeleccionado == "Manicurista" },
                new SelectListItem { Value = "Recepcionista", Text = "Recepcionista", Selected = cargoSeleccionado == "Recepcionista" },
                new SelectListItem { Value = "Gerente", Text = "Gerente", Selected = cargoSeleccionado == "Gerente" },
                new SelectListItem { Value = "Asistente", Text = "Asistente", Selected = cargoSeleccionado == "Asistente" }
            };

            ViewBag.CargosOptions = cargosOptions;

            // ✅ HORARIO OPTIONS - INCLUIR TODOS LOS VALORES POSIBLES
            var horariosOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar horario..." },
                new SelectListItem { Value = "Lun-Vie 8:00-16:00", Text = "Lun-Vie 8:00-16:00" },
                new SelectListItem { Value = "Lun-Vie 9:00-17:00", Text = "Lun-Vie 9:00-17:00" },
                new SelectListItem { Value = "Lun-Vie 10:00-18:00", Text = "Lun-Vie 10:00-18:00" },
                new SelectListItem { Value = "Lun-Sab 9:00-17:00", Text = "Lun-Sab 9:00-17:00" },
                new SelectListItem { Value = "Mar-Sab 9:00-17:00", Text = "Mar-Sab 9:00-17:00" },
                new SelectListItem { Value = "Tiempo Parcial", Text = "Tiempo Parcial" }
            };

            ViewBag.HorariosOptions = horariosOptions;

            Console.WriteLine($"🔍 DEBUG PrepararDropdownData: Cargo seleccionado = '{cargoSeleccionado}'");
        }
    }
}