using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsExtensions
{
    public static string AsJson<TProblemDetail>(this TProblemDetail problemDetails) 
        where TProblemDetail : ProblemDetails => JsonSerializer.Serialize(problemDetails);
}