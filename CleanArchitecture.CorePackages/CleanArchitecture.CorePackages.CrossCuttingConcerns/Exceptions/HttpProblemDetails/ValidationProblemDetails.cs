using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class ValidationProblemDetails : ProblemDetails
{
    public IEnumerable<ValidationExceptionModel> Errors { get; init; }
    public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
    {
        Title = "Validation error";
        Status = StatusCodes.Status400BadRequest;
        Detail = "One or more validation errors occurred.";
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        Errors = errors;
    }

}