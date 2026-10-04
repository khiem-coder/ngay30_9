using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;

namespace QuanLySinhVien.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    public HomeController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalStudents = await _context.Students.CountAsync();
        ViewBag.TotalClasses = await _context.ClassRooms.CountAsync();
        return View();
    }
}
