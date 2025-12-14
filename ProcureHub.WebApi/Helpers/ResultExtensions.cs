using Microsoft.AspNetCore.Mvc;
using ProcureHub.Common;

namespace ProcureHub.WebApi.Helpers;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Error error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = error.Message,
            Detail = error.Code,
            Status = error.Type.ToStatusCode(),
            Extensions = new Dictionary<string, object?>()
        };

        if (error.ValidationErrors is not null)
        {
            problemDetails.Extensions["errors"] = error.ValidationErrors;
        }

        return Results.Problem(problemDetails);
    }

    private static int ToStatusCode(this ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}