using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class BusinessProblemDetails : ProblemDetails // ProblemDetails zaten Microsoft.AspNetCore.Mvc içerisinde tanımlı
{
    public BusinessProblemDetails(string detail)
    {
        Title = "Business rule violation";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest;
        Type = "https://httpstatuses.com/400";
    }
}