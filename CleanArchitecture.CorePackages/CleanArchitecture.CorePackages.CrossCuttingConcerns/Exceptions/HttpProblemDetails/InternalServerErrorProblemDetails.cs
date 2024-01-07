using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class InternalServerErrorProblemDetails : ProblemDetails
{
    public InternalServerErrorProblemDetails(string detail)
    {
        Title = "Internal server error";
        Detail = "Internal server error";
        Status = StatusCodes.Status500InternalServerError;
        Type = "https://httpstatuses.com/internal-server-error";
    }
}