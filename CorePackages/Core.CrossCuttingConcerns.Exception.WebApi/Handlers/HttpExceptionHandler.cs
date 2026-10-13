using Microsoft.AspNetCore.Http;
using Core.CrossCuttingConcerns.Exception.Handlers;
using Core.CrossCuttingConcerns.Exception.Types;
using Core.CrossCuttingConcerns.Exception.WebApi.Extensions;
using Core.CrossCuttingConcerns.Exception.WebApi.HttpProblemDetails;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Core.CrossCuttingConcerns.Exception.WebApi.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    public HttpResponse Response
    {
#pragma warning disable S112 // General or reserved exceptions should never be thrown
        get => _response ?? throw new NullReferenceException(nameof(_response));
#pragma warning restore S112 // General or reserved exceptions should never be thrown
        set => _response = value;
    }

    private HttpResponse? _response;

    public override Task HandleException(BusinessException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        return WriteAsync(new BusinessProblemDetails(businessException.Message));
    }

    public override Task HandleException(ConflictException conflictException)
    {
        Response.StatusCode = StatusCodes.Status409Conflict;
        return WriteAsync(new ConflictProblemDetails(conflictException.Message));
    }

    public override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        return WriteAsync(new ValidationProblemDetails(validationException.Errors));
    }

    public override Task HandleException(AuthorizationException authorizationException)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return WriteAsync(new AuthorizationProblemDetails(authorizationException.Message));
    }

    public override Task HandleException(NotFoundException notFoundException)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return WriteAsync(new NotFoundProblemDetails(notFoundException.Message));
    }

    public override Task HandleException(System.Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return WriteAsync(new InternalServerErrorProblemDetails());
    }

    private Task WriteAsync(ProblemDetails problemDetails)
    {
        problemDetails.Extensions["traceId"] = Response.HttpContext.TraceIdentifier;
        return Response.WriteAsync(problemDetails.ToJson());
    }
}
