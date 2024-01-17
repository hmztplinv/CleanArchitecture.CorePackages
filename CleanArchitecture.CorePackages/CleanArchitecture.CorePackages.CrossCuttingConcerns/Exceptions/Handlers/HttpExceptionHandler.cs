
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class HttpExceptionHandler : ExceptionHandler
{
    private HttpResponse? _response; 
    public HttpResponse Response 
    {
        get => _response ?? throw new ArgumentNullException(nameof(_response)); // response varsa döndür yoksa exception fırlat
        set => _response = value; // response değerini set et
    }
    protected override Task HandleException(BusinessException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        string details = new BusinessProblemDetails(businessException.Message).AsJson();
        return Response.WriteAsync(details);
    }

    protected override Task HandleException(Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        string details = new InternalServerErrorProblemDetails(exception.Message).AsJson();
        return Response.WriteAsync(details);
    }

    protected override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        string details = new ValidationProblemDetails(validationException.Errors).AsJson();
        return Response.WriteAsync(details);
    }
}