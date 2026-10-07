using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models;

public class Student
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã sinh viên")]
    [StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string StudentCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    [Display(Name = "Ngày sinh")]
    public DateTime DateOfBirth { get; set; } = new DateTime(2004, 1, 1);

    [Display(Name = "Giới tính")]
    public string Gender { get; set; } = "Nam";

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(15)]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(200)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn lớp")]
    [Display(Name = "Lớp")]
    public int ClassRoomId { get; set; }

    [Display(Name = "Lớp")]
    public ClassRoom? ClassRoom { get; set; }
    public ICollection<StudentImage> Images { get; set; } = new List<StudentImage>();
}
