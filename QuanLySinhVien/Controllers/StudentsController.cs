using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Controllers;

public class StudentsController : Controller
{
    private const int PageSize = 5;

    // ===== Cấu hình upload =====
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB / ảnh

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public StudentsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Thư mục lưu ảnh: wwwroot/uploads/students
    private string UploadFolder => Path.Combine(_env.WebRootPath, "uploads", "students");

    // GET: Students
    public async Task<IActionResult> Index(string? search, int? classId, int page = 1)
    {
        var query = _context.Students
            .Include(s => s.ClassRoom)
            .Include(s => s.Images)            // lấy kèm ảnh để hiển thị
            .AsQueryable();

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
        var student = await _context.Students
            .Include(s => s.ClassRoom)
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.Id == id);
        return student == null ? NotFound() : View(student);
    }

    // GET: Students/Create
    public async Task<IActionResult> Create()
    {
        await LoadClassesAsync();
        return View(new Student());
    }

    // POST: Students/Create
    // "images" phải trùng với name="images" của <input type="file" multiple> trong Create.cshtml
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("StudentCode,FullName,DateOfBirth,Gender,Email,Phone,Address,ClassRoomId")] Student student,
        List<IFormFile>? images)
    {
        await ValidateUniqueCodeAsync(student);
        await ValidateImagesAsync(images);          // sai định dạng -> thêm lỗi vào ModelState

        if (ModelState.IsValid)
        {
            await SaveImagesAsync(student, images); // lưu file + gắn vào student.Images
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
        var student = await _context.Students.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id);
        if (student != null)
        {
            foreach (var img in student.Images) DeleteFile(img.FileName); // xóa file ảnh trên server
            _context.Students.Remove(student);                           // cascade xóa các dòng StudentImages
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa sinh viên.";
        }
        return RedirectToAction(nameof(Index));
    }

    // =====================================================================
    //                         CÁC HÀM XỬ LÝ UPLOAD
    // =====================================================================

    /// Kiểm tra từng file: đuôi .jpg/.jpeg/.png, dung lượng, và nội dung thật sự là JPG/PNG.
    private async Task ValidateImagesAsync(List<IFormFile>? files)
    {
        if (files == null) return;

        foreach (var f in files)
        {
            if (f.Length == 0) continue; // ô chọn file để trống

            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("images", $"File \"{f.FileName}\" không hợp lệ. Chỉ cho phép ảnh .jpg hoặc .png.");
                continue;
            }
            if (f.Length > MaxFileSize)
            {
                ModelState.AddModelError("images", $"File \"{f.FileName}\" vượt quá 2 MB.");
                continue;
            }
            if (!await HasValidSignatureAsync(f))
            {
                ModelState.AddModelError("images", $"File \"{f.FileName}\" không phải ảnh JPG/PNG hợp lệ.");
            }
        }
    }

    /// Đọc vài byte đầu file để chắc chắn đúng là ảnh (chặn đổi đuôi .exe -> .jpg).
    private static async Task<bool> HasValidSignatureAsync(IFormFile file)
    {
        var header = new byte[8];
        await using var stream = file.OpenReadStream();
        int read = await stream.ReadAsync(header, 0, header.Length);
        if (read < 4) return false;

        bool isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
        bool isJpg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        return isPng || isJpg;
    }

    /// Lưu file xuống ổ đĩa, trả về danh sách tên file (đổi tên bằng Guid để không trùng).
    private async Task<List<string>> SaveFilesAsync(List<IFormFile>? files)
    {
        var names = new List<string>();
        if (files == null) return names;

        Directory.CreateDirectory(UploadFolder);

        foreach (var f in files)
        {
            if (f.Length == 0) continue;

            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(UploadFolder, fileName);

            await using var fs = new FileStream(path, FileMode.Create);
            await f.CopyToAsync(fs);
            names.Add(fileName);
        }
        return names;
    }

    /// Lưu file rồi gắn vào student.Images.
    private async Task SaveImagesAsync(Student student, List<IFormFile>? files)
    {
        foreach (var name in await SaveFilesAsync(files))
            student.Images.Add(new StudentImage { FileName = name });
    }

    private void DeleteFile(string fileName)
    {
        var path = Path.Combine(UploadFolder, Path.GetFileName(fileName));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
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