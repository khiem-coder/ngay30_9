# Quản lý sinh viên - ASP.NET Core MVC (Code First, SQL Server)

## Yêu cầu
- .NET 8 SDK
- SQL Server (LocalDB, SQL Express hoặc SQL Server đầy đủ)

## Chạy dự án
1. Sửa chuỗi kết nối `DefaultConnection` trong `appsettings.json` nếu cần.
2. Cài công cụ EF (một lần): `dotnet tool install --global dotnet-ef`
3. Tạo migration và cơ sở dữ liệu:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Chạy: `dotnet run` rồi mở địa chỉ hiển thị trong terminal.

Khi sửa Model, chạy lại `dotnet ef migrations add <Tên>` và `dotnet ef database update` để đồng bộ CSDL.
