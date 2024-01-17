using Microsoft.AspNetCore.Builder;

public static class ExceptionMiddlewareExtensions
{
    // ExceptionMiddlewareExtensions sınıfı IApplicationBuilder tipinde app parametresi alır ve ConfigureCustomExceptionMiddleware methodunu çalıştırır
    public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}