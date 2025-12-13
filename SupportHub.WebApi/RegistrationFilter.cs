using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using SupportHub.Common;
using SupportHub.Features.Staff.Registration;
using SupportHub.Infrastructure;

namespace SupportHub.WebApi;

// TODO: use domain handler to create Staff record

public class RegistrationFilter(
    ApplicationDbContext dbContext,
    IStaffRegistrationValidator staffRegistrationValidator,
    IRequestHandler<RegisterStaff.Request, Result<string>> registerStaffHandler,
    ILogger<RegistrationFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Get the registration request from the endpoint arguments
        var registration = context.GetArgument<RegisterRequest>(0);

        var cancellationToken = context.HttpContext.RequestAborted;

        var isRegistrationAllowed = await staffRegistrationValidator.IsRegistrationAllowedAsync(registration.Email);
        if (!isRegistrationAllowed)
        {
            logger.LogWarning("Staff registration attempt with unauthorized email: {Email}", registration.Email);
            
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Email", ["This email is not authorized to register."] }
            });
        }

        logger.LogInformation("Allowing registration for staff email: {Email}", registration.Email);

        // TODO: Known issue with email possibly being sent even if transaction failed.
        // Will have to do deeper customization to solve (E.g. write to a queue table in the same transaction,
        // or just completely replace the /register endpoint with own implementation). Not tackling in this sample repo.

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Allow the ASP.Net user registration to proceed
            var results = (Results<Ok, ValidationProblem>)(await next(context))!;
            if (results.Result is not Ok)
            {
                return results;
            }

            await registerStaffHandler.HandleAsync(new RegisterStaff.Request(registration.Email), cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating staff user with email {Email}", registration.Email);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}