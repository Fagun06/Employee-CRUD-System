using Employee.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Employee_CRUD_System.Models;

namespace Employee_CRUD_System.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeeController(EmployeeDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }



        public async Task<IActionResult> Index(string searchName, string searchEmail, string searchMobile, DateTime? searchDOB, int? pageNumber)
        {
            ViewData["searchName"] = searchName;
            ViewData["searchEmail"] = searchEmail;
            ViewData["searchMobile"] = searchMobile;
            ViewData["searchDOB"] = searchDOB;

            var employees = from e in _context.Employees select e;


            if (!string.IsNullOrEmpty(searchName))
            {
                employees = employees.Where(e => e.FirstName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchEmail))
            {
                employees = employees.Where(e => e.Email.Contains(searchEmail));
            }

            if (!string.IsNullOrEmpty(searchMobile))
            {
                employees = employees.Where(e => e.Mobile.Contains(searchMobile));
            }

            if (searchDOB.HasValue)
            {
                employees = employees.Where(e => e.DateOfBirth == searchDOB.Value);
            }

            int pageSize = 10;
            return View(await PaginatedList<EmployeeModel>.CreateAsync(employees.AsNoTracking(), pageNumber ?? 1, pageSize));
           
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeModel employee)
        {
            if (ModelState.IsValid)
            {

                if (employee.PhotoFile != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }


                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + employee.PhotoFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);


                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await employee.PhotoFile.CopyToAsync(fileStream);
                    }


                    employee.PhotoPath = "/images/" + uniqueFileName;
                }


                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeModel employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (employee.PhotoFile != null)
                    {
                        string uniqueFileName = UploadPhoto(employee);
                        employee.PhotoPath = "/images/" + uniqueFileName;
                    }

                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                if (employee.PhotoPath != null)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", employee.PhotoPath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                }
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("Not found");
            }

            return RedirectToAction(nameof(Index));
        }


        private string UploadPhoto(EmployeeModel employee)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + employee.PhotoFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                employee.PhotoFile.CopyTo(fileStream);
            }

            return uniqueFileName;
        }


        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }


    }
}
