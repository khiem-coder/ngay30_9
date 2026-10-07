using Microsoft.EntityFrameworkCore;
using QuanLySinhVien.Models;

namespace QuanLySinhVien.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<StudentImage> StudentImages => Set<StudentImage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.StudentCode)
            .IsUnique();

        modelBuilder.Entity<Student>()
            .HasOne(s => s.ClassRoom)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ClassRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dữ liệu mẫu (seed)
        modelBuilder.Entity<ClassRoom>().HasData(
            new ClassRoom { Id = 1, ClassName = "CNTT01", Major = "Công nghệ thông tin" },
            new ClassRoom { Id = 2, ClassName = "CNTT02", Major = "Công nghệ thông tin" },
            new ClassRoom { Id = 3, ClassName = "KTPM01", Major = "Kỹ thuật phần mềm" },
            new ClassRoom { Id = 4, ClassName = "HTTT01", Major = "Hệ thống thông tin" }
        );

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, StudentCode = "SV001", FullName = "Nguyễn Văn An", DateOfBirth = new DateTime(2004, 3, 15), Gender = "Nam", Email = "an.nv@example.com", Phone = "0901234567", Address = "Hà Nội", ClassRoomId = 1 },
            new Student { Id = 2, StudentCode = "SV002", FullName = "Trần Thị Bình", DateOfBirth = new DateTime(2004, 7, 22), Gender = "Nữ", Email = "binh.tt@example.com", Phone = "0912345678", Address = "Hải Phòng", ClassRoomId = 3 },
            new Student { Id = 3, StudentCode = "SV003", FullName = "Lê Minh Cường", DateOfBirth = new DateTime(2003, 11, 5), Gender = "Nam", Email = "cuong.lm@example.com", Phone = "0923456789", Address = "Đà Nẵng", ClassRoomId = 2 }
        );
    }
}
