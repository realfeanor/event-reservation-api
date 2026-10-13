using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exception.WebApi.HttpProblemDetails;

public class ConflictProblemDetails : ProblemDetails
{
    public ConflictProblemDetails(string detail)
    {
        Title = "Conflict";
        Detail = detail;
        Status = StatusCodes.Status409Conflict;
        Type = "https://example.com/probs/conflict";
    }
}
