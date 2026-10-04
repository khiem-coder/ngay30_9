using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Controllers;

public class StudentsController : Controller
{
    private const int PageSize = 5;
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context) => _context = context;

    // GET: Students  (tìm kiếm + lọc theo lớp + phân trang)
    public async Task<IActionResult> Index(string? search, int? classId, int page = 1)
    {
        var query = _context.Students.Include(s => s.ClassRoom).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.FullName.Contains(search) || s.StudentCode.Contains(search));
        }
        if (classId.HasValue)
        {
            query = query.Where(s => s.ClassRoomId == classId.Value);
        }

        int total = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(total / (double)PageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

        var students = await query
            .OrderBy(s => s.StudentCode)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.ClassId = classId;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Classes = new SelectList(await _context.ClassRooms.ToListAsync(), "Id", "ClassName", classId);

        return View(students);
    }

    // GET: Students/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.Students.Include(s => s.ClassRoom).FirstOrDefaultAsync(s => s.Id == id);
        return student == null ? NotFound() : View(student);
    }

    // GET: Students/Create
    public async Task<IActionResult> Create()
    {
        await LoadClassesAsync();
        return View(new Student());
    }

    // POST: Students/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentCode,FullName,DateOfBirth,Gender,Email,Phone,Address,ClassRoomId")] Student student)
    {
        await ValidateUniqueCodeAsync(student);
        if (ModelState.IsValid)
        {
            _context.Add(student);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }
        await LoadClassesAsync(student.ClassRoomId);
        return View(student);
    }

    // GET: Students/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();
        await LoadClassesAsync(student.ClassRoomId);
        return View(student);
    }

    // POST: Students/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,StudentCode,FullName,DateOfBirth,Gender,Email,Phone,Address,ClassRoomId")] Student student)
    {
        if (id != student.Id) return NotFound();

        await ValidateUniqueCodeAsync(student);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật sinh viên thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Students.AnyAsync(s => s.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await LoadClassesAsync(student.ClassRoomId);
        return View(student);
    }

    // GET: Students/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.Students.Include(s => s.ClassRoom).FirstOrDefaultAsync(s => s.Id == id);
        return student == null ? NotFound() : View(student);
    }

    // POST: Students/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa sinh viên.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadClassesAsync(int? selected = null)
    {
        ViewBag.ClassRoomId = new SelectList(await _context.ClassRooms.OrderBy(c => c.ClassName).ToListAsync(), "Id", "ClassName", selected);
    }

    private async Task ValidateUniqueCodeAsync(Student student)
    {
        bool exists = await _context.Students.AnyAsync(s => s.StudentCode == student.StudentCode && s.Id != student.Id);
        if (exists) ModelState.AddModelError(nameof(Student.StudentCode), "Mã sinh viên đã tồn tại");
    }
}
