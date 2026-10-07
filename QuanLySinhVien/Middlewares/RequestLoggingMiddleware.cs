using System.Diagnostics;
using System.Text.RegularExpressions;

namespace QuanLySinhVien.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    // Khớp /Students/Details/0, /Students/Details/-1 (không phân biệt hoa thường)
    private static readonly Regex DetailsPattern = new(
        @"^/Students/Details/(?<id>-?\d+)/?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Chức năng 1: ghi log request
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        Console.WriteLine($"[{time:yyyy-MM-dd HH:mm:ss.fff}] Method: {method} - Path: {path}");

        // Chức năng 3: chặn id không hợp lệ
        var match = DetailsPattern.Match(path);
        if (match.Success && long.TryParse(match.Groups["id"].Value, out var id) && id <= 0)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Student id không hợp lệ");

            stopwatch.Stop();
            Console.WriteLine($"Status Code: {context.Response.StatusCode} - Blocked by middleware ({stopwatch.ElapsedMilliseconds} ms)");
            Console.WriteLine();   // <-- thêm
            return;
        }

        try
        {
            await _next(context); // chuyển request cho bước tiếp theo
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Status Code: 500 - Exception: {ex.Message} ({stopwatch.ElapsedMilliseconds} ms)");
            Console.WriteLine();   // <-- thêm
            throw;
        }

        // Chức năng 2: ghi log status code sau khi xử lý
        stopwatch.Stop();
        Console.WriteLine($"Status Code: {context.Response.StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
        Console.WriteLine();   // <-- thêm
    }
}