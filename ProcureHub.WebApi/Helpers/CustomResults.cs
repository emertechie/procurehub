using Microsoft.AspNetCore.Http.HttpResults;

namespace ProcureHub.WebApi.Helpers;

public class CustomResults
{
    public static ProblemHttpResult RouteIdMismatch()
    {
        return TypedResults.Problem(
            title: "Route ID mismatch",
            detail: "Route ID does not match request ID",
            statusCode: StatusCodes.Status400BadRequest
        );
    }
}
