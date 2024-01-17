using System.Text.Json;
using Microsoft.AspNetCore.Http;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next; // RequestDelegate, bir sonraki middleware'i temsil eder
    private readonly HttpExceptionHandler _httpExceptionHandler; // HttpExceptionHandler, ExceptionHandler sınıfından türetilmiştir
    private readonly IHttpContextAccessor _httpContextAccessor; // HttpContextAccessor, HttpContext sınıfından türetilmiştir
    private readonly LoggerServiceBase _loggerServiceBase; // LoggerServiceBase, Serilog sınıfından türetilmiştir
    public ExceptionMiddleware(RequestDelegate next,IHttpContextAccessor httpContextAccessor,LoggerServiceBase loggerServiceBase)
    {
        _next = next;
        _httpExceptionHandler = new HttpExceptionHandler();
        _httpContextAccessor = httpContextAccessor;
        _loggerServiceBase = loggerServiceBase;
    }

    public async Task InvokeAsync(HttpContext httpContext) // InvokeAsync, bir sonraki middleware'i temsil eder
    {
        try
        {
            await _next(httpContext); // bir sonraki middleware'i çalıştır
        }
        catch (Exception exception)
        {
            await LogException(httpContext, exception); // hata oluşursa LogException methodunu çalıştır
            await HandleExceptionAsync(httpContext.Response, exception); // hata oluşursa HandleExceptionAsync methodunu çalıştır
        }
    }

    private Task LogException(HttpContext httpContext, Exception exception)
    {
        List<LogParameter> logParameters = new()
        {
            new LogParameter{Type=httpContext.GetType().Name, Value=exception.ToString()}
        };

        LogDetailWithException logDetailWithException = new()
        {
            ExceptionMessage = exception.Message,
            MethodName = _next.Method.Name,
            Parameters = logParameters,
            User = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "?"
        };
        _loggerServiceBase.Error(JsonSerializer.Serialize(logDetailWithException));
        return Task.CompletedTask;
    }
    private Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = "application/json";
        _httpExceptionHandler.Response = response;
        return _httpExceptionHandler.HandleExceptionAsync(exception); // oluşan exception'ı HttpExceptionHandler sınıfındaki HandleExceptionAsync methoduna gönder
        // HandleExceptionAsync methodu exception'ı switch case ile kontrol eder ve ilgili methodu çalıştırır
    }
}