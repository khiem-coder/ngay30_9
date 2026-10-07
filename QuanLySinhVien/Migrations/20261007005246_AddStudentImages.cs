using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLySinhVien.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Major = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClassRoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ClassRooms",
                columns: new[] { "Id", "ClassName", "Major" },
                values: new object[,]
                {
                    { 1, "CNTT01", "Công nghệ thông tin" },
                    { 2, "CNTT02", "Công nghệ thông tin" },
                    { 3, "KTPM01", "Kỹ thuật phần mềm" },
                    { 4, "HTTT01", "Hệ thống thông tin" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Address", "ClassRoomId", "DateOfBirth", "Email", "FullName", "Gender", "Phone", "StudentCode" },
                values: new object[,]
                {
                    { 1, "Hà Nội", 1, new DateTime(2004, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "an.nv@example.com", "Nguyễn Văn An", "Nam", "0901234567", "SV001" },
                    { 2, "Hải Phòng", 3, new DateTime(2004, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "binh.tt@example.com", "Trần Thị Bình", "Nữ", "0912345678", "SV002" },
                    { 3, "Đà Nẵng", 2, new DateTime(2003, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "cuong.lm@example.com", "Lê Minh Cường", "Nam", "0923456789", "SV003" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassRoomId",
                table: "Students",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentCode",
                table: "Students",
                column: "StudentCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "ClassRooms");
        }
    }
}
