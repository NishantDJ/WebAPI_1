using System.Net.WebSockets;
using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebAPI_1.Data;
using WebAPI_1.Exceptions;
using WebAPI_1.Model;

namespace WebAPI_1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICacheProvider _cache;
        public EmployeesController(AppDbContext context, ICacheProvider cache)
        {
            _context = context;
            _cache = cache;
        }

        // =========================
        // GET: api/employees
        // =========================
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployees(
      int page = 1,
      int pagesize = 10)
        {
            // Safety check (optional but recommended)
            if (page <= 0 || pagesize <= 0)
                return BadRequest("Page and PageSize must be greater than zero.");

            // Dynamic cache key (page + pagesize based)
            var cacheKey = $"employees:v1:page={page}:size={pagesize}";

            // Final response variable
            List<EmployeeResponseDto> employees;

            // Try to get data from cache
            if (!_cache.TryGetValue(cacheKey, out employees))
            {
                // Cache MISS → DB hit
                employees = await _context.Employees
                    .AsNoTracking()
                    .Where(e => e.IsActive)
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
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

                // Cache options
                var cacheOptions = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))   // Frequently used → stay alive
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));  // Max lifetime

                // Store in cache
                _cache.Set(cacheKey, employees, cacheOptions);
            }

            // Always return 200 OK (never null → no 204)
            return Ok(employees ?? new List<EmployeeResponseDto>());
        }

        // =========================
        // GET: api/employees/{id}
        // =========================
        [HttpGet("{id}")]
            public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(int id)
            {
                var employee = await _context.Employees
                    .AsNoTracking()
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
                    throw new BusinessException("Employee not found.");

                return Ok(employee);
            }

            // =========================
            // POST: api/employees
            // =========================
            [HttpPost]
            public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
            {
                var emailExists = await _context.Employees
                    .AnyAsync(e => e.Email == dto.Email);

                if (emailExists)
                    throw new BusinessException("Employee with this email already exists.");

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

                return CreatedAtAction(nameof(GetEmployeeById),
                    new { id = employee.Id }, response);
            }

            // =========================
            // PUT: api/employees/{id}
            // =========================
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null || !employee.IsActive)
                    throw new BusinessException("Employee not found or inactive.");

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
                    throw new BusinessException("Employee not found or already deleted.");

                employee.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    }
