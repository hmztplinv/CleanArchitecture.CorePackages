using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsExtensions
{
    public static string AsJson<TProblemDetail>(this TProblemDetail problemDetails) // TProblemDetail tipinde problemDetails nesnesi alır
        where TProblemDetail : ProblemDetails => JsonSerializer.Serialize(problemDetails); // problemDetails nesnesini json formatına çevirir
}