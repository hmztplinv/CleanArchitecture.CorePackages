public abstract class ExceptionHandler // abstract class cunku bu class'i inherit eden class'larin HandleException methodlarini implement etmesi gerekiyor
{
    public Task HandleExceptionAsync(Exception exception) => 
        exception switch // hata cesitlerine gore HandleException methodlarini cagiriyor
        {
            BusinessException businessException => HandleException(businessException),
            ValidationException validationException => HandleException(validationException),
            _ => HandleException(exception) // default case
        };
    protected abstract Task HandleException(BusinessException businessException); // abstract methodlar inherit edildiginde implement edilmek zorunda
    protected abstract Task HandleException(ValidationException validationException);
    protected abstract Task HandleException(Exception exception);
}