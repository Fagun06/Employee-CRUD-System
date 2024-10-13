
using Employee_CRUD_System.Models;
using Microsoft.EntityFrameworkCore;


namespace Employee.Data
{

    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<EmployeeModel> Employees { get; set; }


    }

}