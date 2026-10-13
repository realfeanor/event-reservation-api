using Core.CrossCuttingConcerns.Exception.Types;
using Domain.Exceptions;
using MediatR;

namespace Application.Pipelines.DomainRules;

public class DomainRuleExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await next();
        }
        catch (DomainRuleException exception)
        {
            throw new BusinessException(exception.Message, exception);
        }
    }
}
