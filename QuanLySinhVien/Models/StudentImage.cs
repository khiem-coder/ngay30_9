namespace QuanLySinhVien.Models;

// Mỗi dòng = 1 ảnh của 1 sinh viên. Chỉ lưu TÊN FILE trong DB, file thật nằm ở wwwroot/uploads/students
public class StudentImage
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public Student? Student { get; set; }
}