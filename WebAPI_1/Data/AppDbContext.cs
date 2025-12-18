using Microsoft.EntityFrameworkCore;
using WebAPI_1.Model;

namespace WebAPI_1.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Employee> Employees => Set<Employee>();
    }
}
