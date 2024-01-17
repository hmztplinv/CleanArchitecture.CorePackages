using FluentValidation;
using MediatR;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> 
{
    private readonly IEnumerable<IValidator<TRequest>> _validators; // IValidator<TRequest> tipinde bir liste oluşturuluyor.

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    // Aşagıdaki kodu açıklamak gerekirse;
    // 1. ValidationContext nesnesi oluşturuluyor.
    // 2. Bu nesne içerisine request nesnesi ekleniyor.
    // 3. _validators içerisindeki tüm validatorler tek tek geziliyor.
    // 4. Gezilen validatorler Validate metoduna ValidationContext nesnesi gönderiliyor.
    // 5. Validate metodundan dönen sonuçlar IEnumerable<ValidationFailure> tipinde dönüyor.
    // 6. Bu sonuçlar ValidationFailure nesnesi içerisinde PropertyName ve ErrorMessage olarak ayrılıyor.
    // 7. Daha sonra bu sonuçlar ValidationExceptionModel nesnesi içerisine Property ve Errors olarak ayrılıyor.
    // 8. Eğer hata varsa ValidationException fırlatılıyor.
    // 9. Eğer hata yoksa next() metodu ile bir sonraki pipeline'a geçiliyor.

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);
        IEnumerable<ValidationExceptionModel> errors = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .GroupBy(
                keySelector: p => p.PropertyName,
                resultSelector: (propertyName, errors) => new ValidationExceptionModel
                {
                    Property = propertyName,
                    Errors = errors.Select(x => x.ErrorMessage)
                }).ToList();
        
        if (errors.Any())
            throw new ValidationException(errors);
        TResponse response = await next();
        return response;
        
    }
}