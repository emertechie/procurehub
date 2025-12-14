using Microsoft.EntityFrameworkCore;
using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Staff;

public static class ListStaff
{
    public record Request(string? Email);

    public record Response(
        string Id,
        string Email,
        int? DepartmentId,
        string? DepartmentName);

    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response[]>
    {
        public Task<Response[]> HandleAsync(Request request, CancellationToken token)
        {
            var query = dbContext.Staff
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                // Note: emails always stored in lowercase
                var lowerCasedEmail = request.Email.ToLowerInvariant();
                query = query.Where(s => s.User.Email == lowerCasedEmail);    
            }

            return query.Select(s => new Response(
                    s.UserId,
                    s.User.Email!,
                    s.DepartmentId,
                    s.Department != null ? s.Department.Name : null))
                .ToArrayAsync(token);
        }
    }
}