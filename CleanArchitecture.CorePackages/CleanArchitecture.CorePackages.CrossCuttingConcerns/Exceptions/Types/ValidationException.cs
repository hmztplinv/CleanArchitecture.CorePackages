public class ValidationException : Exception
{
    public IEnumerable<ValidationExceptionModel> Errors { get; } // Hata mesajlarını tutan liste.sadece get çünkü readonly.

    public ValidationException() : base()
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }
    public ValidationException(string? message) : base(message)
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }
    public ValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
        Errors = Array.Empty<ValidationExceptionModel>();
    }
    public ValidationException(IEnumerable<ValidationExceptionModel> errors) : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }
    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors) // Hata mesajlarını oluşturan metot.
    {
        IEnumerable<string> arr= errors.Select(
            x => $"{Environment.NewLine} -- {x.Property} : {string.Join(Environment.NewLine, values: x.Errors ?? Array.Empty<string>())}");
        return $"Validation failed: {string.Join(string.Empty, arr)}";
    }
}
// Aşagıdaki kodu açıklamak gerekirse;
// 1. Property ve Errors alanlarına sahip bir nesne oluşturuluyor.
// 2. Property alanı hata olan property'nin adını tutuyor.
// 3. Errors alanı ise hata mesajlarını tutuyor.
// 4. Örneğin bir property'nin birden fazla hatası olabilir. Bu yüzden Errors alanı IEnumerable<string> olarak tanımlanmıştır.

public class ValidationExceptionModel
{
    public string? Property { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}