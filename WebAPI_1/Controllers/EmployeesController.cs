using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI_1.Data;
using WebAPI_1.Model;

namespace WebAPI_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: api/employees
        // =========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployees()
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    Department = e.Department,
                    Salary = e.Salary,
                    DateOfJoining = e.DateOfJoining,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            return Ok(employees);
        }

        // =========================
        // GET: api/employees/{id}
        // =========================
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(int id)
        {
            var employee = await _context.Employees
                .Where(e => e.Id == id && e.IsActive)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    Department = e.Department,
                    Salary = e.Salary,
                    DateOfJoining = e.DateOfJoining,
                    IsActive = e.IsActive
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // =========================
        // POST: api/employees
        // =========================
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                Salary = dto.Salary,
                DateOfJoining = dto.DateOfJoining,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var response = new EmployeeResponseDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                Salary = employee.Salary,
                DateOfJoining = employee.DateOfJoining,
                IsActive = employee.IsActive
            };

            return CreatedAtAction(
                nameof(GetEmployeeById),
                new { id = employee.Id },
                response
            );
        }

        // =========================
        // PUT: api/employees/{id}
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null || !employee.IsActive)
                return NotFound();

            employee.Name = dto.Name;
            employee.Department = dto.Department;
            employee.Salary = dto.Salary;
            employee.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // DELETE: api/employees/{id}
        // (Soft Delete)
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null || !employee.IsActive)
                return NotFound();

            employee.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
