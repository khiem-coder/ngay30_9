using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models;

public class ClassRoom
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Tên lớp")]
    public string ClassName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Ngành")]
    public string? Major { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
